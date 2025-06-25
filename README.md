# Woopi AI Doc Analyzer

## 📚 Guia de Navegação

- [Introdução](#introdução)
- [Pré requisitos: Front-end](#pré-requisitos-front-end)
- [Pré requisitos: Back-end](#pré-requisitos-back-end)
- [Rodando o Front-end](#rodando-o-front-end)
- [Rodando o Back-end](#rodando-o-back-end)
- [Acessando Ambiente AKS (Argo)](#acessando-ambiente-aks-argo)
- [Recursos Doc Analyzer](#recursos-doc-analyzer)

## Introdução

Doc Analyzer é uma aplicação desenvolvida com Vue.js no front-end e .NET 6 no back-end, estruturada com base nos princípios da Clean Architecture. A aplicação é dividida em cinco camadas:

* **API**: Contém os controllers, responsáveis por receber e encaminhar as requisições HTTP;
* **Domain**: Agrupa as entidades, DTOs e demais objetos de domínio;
* **Application**: Abriga os services que implementam a lógica de negócio da aplicação;
* **Repository**: Responsável pelo acesso a dados, com implementações que interagem diretamente com o banco de dados;
* **Tests**: Contém os testes de unidade, focados principalmente nos services da camada de Application, garantindo a validação da lógica de negócio.

  ![Arch Doc Analyzer (5)](https://github.com/user-attachments/assets/24d996a6-57eb-4c63-bce7-eebd54d3d21a)

  ![image](https://github.com/user-attachments/assets/71003395-7fec-4cb2-9ca2-7b3cdb102e38)

  ![image](https://github.com/user-attachments/assets/62c7ff9e-a665-4ef9-9b43-7621e2c63686)

## Pré requisitos: Front-end

* #### **Node.js / npm**
    O **npm** é instalado automaticamente com o **Node.js**. Baixe a versão mais recente do Node.js [aqui](https://nodejs.org/pt/download).
  
    Após instalar o Node.js, verifique se está instalado corretamente com os comandos:
  
   ```bash
   node --version
   npm --version
   ```

## Pré requisitos: Back-end

* #### **Visual Studio 2022 - Professional**

    O primeiro passo é instalar o Visual Studio 2022 Professional. Para isso, entre em contato com seu gestor e solicite uma chave de licença válida para iniciar o download. Após a instalação, é importante selecionar as seguintes cargas de trabalho (outras são opcionais):
    
    ![image](https://github.com/user-attachments/assets/f950a2b3-5fa2-4369-a000-41e6e3e2d918)

    Para os componentes individuais, certifique-se de marcar as opções correspondentes ao .NET SDK e .NET Runtime compatíveis com a versão utilizada no projeto. Detalharemos os requisitos específicos do .NET na próxima seção.

* #### **.Net**

    Atualmente, nossos projetos utilizam versões do .NET que variam da 6.x até a 8.x, portanto, é importante garantir que essas versões estejam instaladas em sua máquina. Você pode realizar o download diretamente pelo site oficial da Microsoft: [Download .Net](https://dotnet.microsoft.com/pt-br/download)
  
* #### **SQL Server Management Studio (SSMS)**

    É altamente recomendável instalar o SQL Server Management Studio (SSMS), pois ele facilita o acesso e a manipulação dos bancos de dados utilizados no projeto. Você pode fazer o download através do site oficial da Microsoft: [Download SSMS](https://learn.microsoft.com/en-us/ssms/download-sql-server-management-studio-ssms)

* #### **Microsoft Azure Storage Explorer**

    Caso o Microsoft Azure Storage Explorer não esteja instalado em sua máquina, você pode fazer o download através do seguinte link: [Download Azure Storage Explorer]([https://dotnet.microsoft.com/pt-br/download](https://azure.microsoft.com/en-us/products/storage/storage-explorer/)).
    Ao acessar a página, selecione o seu sistema operacional e siga as instruções para concluir a instalação do programa.

    ![image](https://github.com/stefanini-applications/woopiai-docanalyzer/assets/124811672/45f8f9b8-04f9-4e2f-8583-429d9d10ef80)

* #### Azurite

    Caso o Azurite ainda não esteja instalado em sua máquina, é necessário realizar a instalação via **NPM** utilizando o terminal. Execute o seguinte comando:

     ```bash
     npm install -g azurite
     ```
     
     Depois de instalar o Azurite, sempre que você for executar o projeto Functions.FileRepository, antes é necessário rodar o Azurite com o seguinte comando:

     ```bash
     azurite --silent --location c:\azurite --debug c:\azurite\debug.log
     ```

* #### Function runtime

  Antes de iniciar a configuração do projeto, verifique se o **runtime version** do .NET está na versão 4 ou superior. Para isso, abra o **CMD** e execute o seguinte comando: 

  ```bash
  `func --version`
  ```

  Caso a versão apresentada seja inferior a 4, rode o comando a seguir no **CMD** para atualizar:

   ```bash
  `npm install -g azure-functions-core-tools@4.0.5198`
   ```

  **Observação: Caso você nunca tenha utilizado funções, será necessário buildar o projeto FileRepository.Functions para que o runtime seja instalado. Se, mesmo assim ele não for instalado, acesse o site: [site de instalação](https://github.com/Azure/azure-functions-core-tools).

  E faça o download do runtime para seu sistema operacional. Depois de instalado execute os passos mencionados acima.

  ![image](https://github.com/user-attachments/assets/649264c3-35c1-4700-b981-b206d33f8466)

## Rodando o Front-end

   1. Abra o terminal (CMD, PowerShell ou terminal integrado do Visual Studio).
   2. Navegue até a pasta do projeto front-end (chamada de `vueapp`):
   3. Instale as dependências do projeto:
      
   ```bash
   `npm install`
   ```

   4. Inicie o servidor de desenvolvimento:
 
   ```bash
   `npm run serve`
   ```
     
## Rodando o Back-end

  1. #### Banco de dados
     
     O banco de dados, por padrão, está hospedado no Azure. Para acessar o banco do ambiente de **DEV**, seu IP precisa estar liberado.
     * Acesse o site [https://www.meuip.com.br](https://www.meuip.com.br) para descobrir seu IP.
     * Informe esse IP a um colaborador que tenha acesso ao diretório do banco na Azure.
     * Solicite também a **connection string** de acesso.

     ⚠️ **IMPORTANTE:**
     
     No entanto, **para rodar localmente**, não é necessário utilizar o banco de DEV.  
     Basta acessar o **Doc Analyzer**, fazer o login, e o sistema criará o banco automaticamente usando a connection string definida no `appSettings`.

   3. ### Ajustes no AppSettings

   O arquivo **appsettings.json** que está no projeto DocAnalyzer.Api estão sem algumas chaves secretas para utilização de alguns serviços.

   ![image](https://github.com/stefanini-applications/woopiai-docanalyzer/assets/124811672/c860ab16-7855-4f8a-a123-455980576436)

   ### Abaixo estão os valores para as chaves secretas do appsettings.json:

   |CHAVE                         |VALOR                                                      |
   -------------------------------|-----------------------------                              |
   `"TemplateConnection"`         |"Conexão customizada com o banco de dados"(exemplo abaixo) |
   `"EmbeddingsApiKey"`           |"tNgMjmgQs3qpk1gZMRPDIKgnNQOQ11cC1nihg9oiw88=internal"     |
   `"FunctionApiKey"`             |"p4v1xeZ9VVamcSJzDuD8U_Id7VoZnyKx4N0qgK_D5z0HAzFu7DwxFg==" |
   `"OCRApiKey"`                  |"097234f62b1e40b9aa016606212bb430"                         |
   `"OCREndpoint`"                |"https://newocrprod.cognitiveservices.azure.com/           |
   `"Key"` (JWT)                  |"ACDt1vR3lXToPQ1g3MyNFsd3S4Ksz31l"                         |
   `"KeyAccess"`                  |"y4cBbT4dt5yGEgPjrOAXaf1yxUSKJCb2"                         |
   `"ClientId"`                   |"afe3cca3-f48a-4ef7-a4fc-355a8b8b312d"                     |

   ```json
   {
     "ConnectionStrings": {
       "TemplateConnection": "Data Source=.\\SQLEXPRESS;Initial Catalog=___NEWDB___;MultipleActiveResultSets=true;user=sa;password=123;TrustServerCertificate=True"
     },
   } 
```

   3. ### Ajustes no FileRepository.Functions

   - O primeiro passo é criar um arquivo do tipo json, que vai se chamar local.settings.json. Este arquivo vai conter as informações necessárias para a função rodar localmente. Seu projeto deve ficar nessa estrutura já com o arquivo local.settings.json criado:

   ![image](https://github.com/stefanini-applications/woopiai-docanalyzer/assets/124811672/e9b48d4e-18d6-4cd0-b048-cbf43f4590d3)

   Neste arquivo criado você deve colocar o seguinte código:

   ```json
   {
     "IsEncrypted": false,
     "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "AzureStorageOptions__ConnectionString": "UseDevelopmentStorage=true",
        "AzureStorageOptions__ContainerName": "files-dev"
     },
     "Host": {
        "CORS": "http://localhost:8080"
     }
   } 
   ```

   - Depois de o arquivo ser criado, se o Azurite não estiver rodando, rode o comando no **CMD**
   ```
   azurite --silent --location c:\azurite --debug c:\azurite\debug.log
   ```

   - Com o Azurite rodando, vá até o programa instalado Microsoft Azure Storage Explorer, e no menu lateral esquerdo, clique em Local & Attached -> Emulator -> Blob Containers

   ![image](https://github.com/stefanini-applications/woopiai-docanalyzer/assets/124811672/1e3eed06-ee5f-4cc7-bcf9-3e24c0ca6142)

   - Clique com o botão direito em Blob Containers e crie um novo. (o nome dele ser o mesmo da variável no arquivo json AzureStorageOptions__ContainerName)

   ![image](https://github.com/stefanini-applications/woopiai-docanalyzer/assets/124811672/43f95384-be89-4d54-b029-1ea67a07b31e)

   **Agora todas as vezes que um upload de arquivo for realizado, ele será salvo neste blob Container.**

   - Com esse passos realizados, basta acessar o visual studio e inicializar os projetos a seguir para o back-end funcionar:

   ![image](https://github.com/stefanini-applications/woopiai-docanalyzer/assets/124811672/61bc01d1-7b95-4391-a1bf-dc1f868f6d1a)

   **Para verificar se a function está rodando, basta visualizar o cmd do projeto FileRepository.Functions e identificar a URL que ela está sendo executada como no exemplo a seguir:**

   ![image](https://github.com/stefanini-applications/woopiai-docanalyzer/assets/124811672/2bf51241-443c-46a1-8ce7-42fe7ff49f79)

## Acessando ambiente AKS (Argo)

   1. #### DEV
      
   - Doc Analyzer (Back-end): https://argocd.sophieagent.com/applications/woopi-ai-doc-back-end-aks-woopi-dev
   - Doc Analyzer (Front-end): https://argocd.sophieagent.com/applications/woopi-ai-doc-front-end-aks-woopi-dev
   - File Repository: https://argocd.sophieagent.com/applications/woopi-ai-doc-file-repository-aks-woopi-dev
   
   2. #### QA
      
   - Doc Analyzer (Back-end): https://argocd.sophieagent.com/applications/woopi-ai-doc-back-end-aks-woopi-qa
   - Doc Analyzer (Front-end): https://argocd.sophieagent.com/applications/woopi-ai-doc-front-end-aks-woopi-qa
   - File Repository: https://argocd.sophieagent.com/applications/woopi-ai-doc-file-repository-aks-woopi-qa

   3. #### Produção
      
   - Doc Analyzer (Back-end): https://argocd.sophieagent.com/applications/woopi-ai-doc-back-end-aks-woopi-prod
   - Doc Analyzer (Front-end): https://argocd.sophieagent.com/applications/woopi-ai-doc-front-end-aks-woopi-prod
   - File Repository: https://argocd.sophieagent.com/applications/woopi-ai-doc-file-repository-aks-woopi-prod

## Recursos Doc Analyzer

   - Todos os recursos estão hospedados na Azure, no diretório Woopi AI, incluindo Azure Functions, storages e bancos de dados. O único recurso fora desse diretório é o aplicativo de SSO, que está no diretório Sophie Chat.




