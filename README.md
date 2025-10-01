# Stock Quote Alert

Este é um programa de console que monitora a cotação de um ativo da B3 e envia um alerta por email quando o preço atinge limites definidos pelo usuário.

## Pré-requisitos

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (ou superior)

## Como Configurar e Rodar

1.  **Clone o repositório:**
    ```sh
    git clone [https://github.com/seu-usuario/seu-repositorio.git](https://github.com/seu-usuario/seu-repositorio.git)
    cd seu-repositorio/ConsoleApp
    ```

2.  **Crie e configure seu arquivo `config.json`:**
    Copie o arquivo de exemplo para criar seu próprio arquivo de configuração.
    
    *No Windows (CMD/PowerShell):*
    ```sh
    copy config.example.json config.json
    ```
    *No Linux/macOS:*
    ```sh
    cp config.example.json config.json
    ```

3.  **Edite o `config.json`** com suas próprias credenciais de email (lembre-se de usar uma Senha de App do Gmail) e o email do destinatário.

4.  **Compile o projeto:**
    Para garantir que todas as dependências foram baixadas e criar a versão final do programa, execute:
    ```sh
    dotnet build -c Release
    ```

5.  **Execute o programa:**
    Navegue até a pasta onde o executável foi gerado e rode o programa passando os argumentos: `<ATIVO> <MAXIMO> <MINIMO>`.

    ```sh
    cd bin/Release/net8.0
    .\stock-quote-alert.exe PETR4 22.67 22.59
    ```

---

### **Resumo do Fluxo para o Usuário**

Com essa estrutura, o fluxo que você descreveu funcionará perfeitamente:

1.  Usuário executa `git clone ...`.
2.  Navega até a pasta `ConsoleApp`.
3.  Copia `config.example.json` para `config.json`.
4.  Abre o `config.json` e insere suas credenciais.
5.  Roda `dotnet build -c Release`.
6.  Entra na pasta `bin/Release/net8.0`.
7.  Executa `.\stock-quote-alert.exe PETR4 22.67 22.59`.

Essa organização é profissional, segura e torna seu projeto muito mais fácil de ser utilizado por outras pessoas.