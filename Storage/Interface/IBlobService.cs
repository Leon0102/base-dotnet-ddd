namespace Storage.Interface;

public interface IBlobService
{
    string GenerateBlobSasUriAsync(string blobName, TimeSpan expiryTime);
}