# PangToolsNet
Ferramentas .NET para Pangya

[UpdateList]
Baseado na biblioteca de: DaveDevil's - (Obrigado por compartilhar)

Exemplo de uso: 

string filePath = @"C:\\Users\\yourUserName\\desktop\\updatelist";
string fileOutput = filePath + ".xml" ;

//Decript
var decrypted = new FileCrypt().DecryptEncryptFile(filePath, FileCrypt.KeyEnum.JP, FileCrypt.OperacaoEnum.Decrypt);
System.IO.File.WriteAllBytes(fileOutput, decrypted);

//Encrypt
var encrypted = new FileCrypt().DecryptEncryptFile(fileOutput, FileCrypt.KeyEnum.JP, FileCrypt.OperacaoEnum.Encrypt);
System.IO.File.WriteAllBytes(filePath + "_encrypt", encrypted);
