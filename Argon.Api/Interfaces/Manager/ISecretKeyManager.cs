namespace Argon.Api.Interfaces.Manager
{
	public interface ISecretKeyManager
	{
		T ProcessLoad<T>(T obj);
		T ProcessSave<T>(T obj);

		string Encrypt(string text);

		string Decrypt(string text);
	}
}
