using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Storage.Interface;

namespace Storage.Implementation;

public class BlobService : IBlobService
{
    private readonly string storageConnectionString = "<YourStorageConnectionString>";
    private readonly string containerName = "mycontainer";
    private IBlobService _blobServiceImplementation;


    public string GenerateBlobSasUri(string blobName, TimeSpan expiryTime)
    {
        // Create a BlobServiceClient using the connection string
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

        // Get the container client
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        // Get the blob client
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        // Check if the blob exists
        if (!blobClient.Exists())
        {
            throw new Exception($"Blob '{blobName}' does not exist.");
        }

        // Generate a SAS token for the blob
        if (blobClient.CanGenerateSasUri)
        {
            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",  // "b" for blob, "c" for container
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiryTime) // Expiry time for the SAS URL
            };

            // Set permissions (e.g., read, write, delete, etc.)
            sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

            // Generate the SAS token and append it to the blob URL
            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri.ToString(); // Return the full URL with SAS token
        }
        else
        {
            throw new InvalidOperationException("SAS generation is not enabled for this blob.");
        }
    }

    public string GenerateBlobSasUriAsync(string blobName, TimeSpan expiryTime)
    {
        throw new NotImplementedException();
    }
}