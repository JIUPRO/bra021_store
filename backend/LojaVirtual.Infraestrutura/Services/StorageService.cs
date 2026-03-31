using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace LojaVirtual.Infraestrutura.Services
{
	public interface IStorageService
	{
		Task<StorageUploadResponse> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
		Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default);
	}

	public class StorageService : IStorageService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public StorageService(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_configuration = configuration;
		}

		public async Task<StorageUploadResponse> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
		{
			var token = await GetTokenAsync(cancellationToken);

			using var form = new MultipartFormDataContent();
			form.Add(new StringContent(GetBucket()), "bucket");
			var streamContent = new StreamContent(fileStream);
			streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
			form.Add(streamContent, "file", fileName);

			using var request = new HttpRequestMessage(HttpMethod.Post, GetFilesUrl());
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			request.Content = form;

			var response = await _httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<StorageUploadResponse>(cancellationToken: cancellationToken);
			return result ?? throw new InvalidOperationException("Resposta inválida do serviço de storage.");
		}

		public async Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(fileKey))
			{
				return false;
			}

			var token = await GetTokenAsync(cancellationToken);

			using var request = new HttpRequestMessage(HttpMethod.Delete, $"{GetFilesUrl().TrimEnd('/')}/{GetBucket()}/{fileKey}");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var response = await _httpClient.SendAsync(request, cancellationToken);
			return response.IsSuccessStatusCode;
		}

		private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
		{
			var payload = new
			{
				bucket = GetBucket(),
				login = _configuration["StorageSettings:Login"] ?? string.Empty,
				password = _configuration["StorageSettings:Password"] ?? string.Empty
			};

			var response = await _httpClient.PostAsJsonAsync(GetAuthUrl(), payload, cancellationToken);
			response.EnsureSuccessStatusCode();

			await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
			if (document.RootElement.TryGetProperty("token", out var tokenElement))
			{
				return tokenElement.GetString() ?? throw new InvalidOperationException("Token inválido no serviço de storage.");
			}

			throw new InvalidOperationException("Token não encontrado na resposta do serviço de storage.");
		}

		private string GetBucket()
		{
			return _configuration["StorageSettings:Bucket"]
				?? throw new InvalidOperationException("StorageSettings:Bucket não configurado.");
		}

		private string GetAuthUrl()
		{
			return GetRequiredAbsoluteUrl("StorageSettings:AuthUrl");
		}

		private string GetFilesUrl()
		{
			return GetRequiredAbsoluteUrl("StorageSettings:FilesUrl");
		}

		private string GetRequiredAbsoluteUrl(string key)
		{
			var value = _configuration[key];
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new InvalidOperationException($"{key} não configurado.");
			}

			if (!Uri.TryCreate(value, UriKind.Absolute, out _))
			{
				throw new InvalidOperationException($"{key} deve ser uma URL absoluta válida.");
			}

			return value;
		}
	}

	public class StorageUploadResponse
	{
		public string Url { get; set; } = string.Empty;
		public string Key { get; set; } = string.Empty;
	}
}
