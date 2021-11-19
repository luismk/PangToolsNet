# PangToolsNet
Ferramentas .NET para Pangya

[UpdateList]
Baseado na biblioteca de: DaveDevil's - (Obrigado por compartilhar)

Exemplo de uso:    

[Uso do Método DecryptEncryptFile]

var result = new FileCrypt().DecryptEncryptFile(filePath, out byte[] decrypted, (FileCrypt.KeyEnum)i);

[Informações DecryptEncryptFile]

filePath = local onde o arquivo está 

decrypted = objeto com dados encriptografados ou decriptografados

i = Chave Key Enum(usado somente para girar até encontrar uma chave compatível)

result = gera um resultado do tipo (sucesso, testar nova chave, error, falied)
