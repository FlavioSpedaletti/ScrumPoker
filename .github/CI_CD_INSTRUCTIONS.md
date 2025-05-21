# Instruções para Configuração da Esteira de CI/CD

Este documento explica como configurar a integração contínua e implantação contínua (CI/CD) para o projeto ScrumPoker no Azure usando GitHub Actions.

## Pré-requisitos

1. Projeto hospedado no GitHub
2. Aplicativo já implantado no Azure Web App
3. Acesso administrativo ao repositório GitHub e ao Azure Web App

## Configuração dos Segredos no GitHub

Para que o workflow funcione, você precisa configurar dois segredos no seu repositório GitHub:

1. **AZURE_WEBAPP_NAME**: O nome do seu aplicativo no Azure Web App (sem a parte `.azurewebsites.net`)
2. **AZURE_WEBAPP_PUBLISH_PROFILE**: O conteúdo do perfil de publicação do Azure

### Como obter o perfil de publicação do Azure:

1. Acesse o [Portal do Azure](https://portal.azure.com/)
2. Navegue até o seu Web App
3. No menu do Web App, clique em "Visão geral"
4. Clique no botão "Baixar perfil de publicação"
5. Abra o arquivo baixado em um editor de texto
6. Copie todo o conteúdo do arquivo (é um arquivo XML)

### Como configurar os segredos no GitHub:

1. No seu repositório GitHub, clique na aba "Settings"
2. No menu lateral, clique em "Secrets and variables" e depois em "Actions"
3. Clique em "New repository secret"
4. Adicione o segredo `AZURE_WEBAPP_NAME` com o nome do seu Web App
5. Adicione outro segredo `AZURE_WEBAPP_PUBLISH_PROFILE` com o conteúdo do perfil de publicação

## Como funciona o workflow

O workflow configurado no arquivo `.github/workflows/azure-deploy.yml` fará o seguinte:

1. Será acionado automaticamente quando houver um push na branch `master`
2. Pode ser acionado manualmente seguindo estes passos:
   - Acesse o repositório no GitHub
   - Clique na aba "Actions"
   - Selecione o workflow "Deploy to Azure"
   - Clique no botão "Run workflow"
   - Selecione a branch desejada e confirme
3. Configura o ambiente .NET 9.0 Preview
4. Restaura dependências, compila e publica o projeto
5. Implanta o aplicativo no Azure Web App usando o perfil de publicação

## Verificando a execução do workflow

1. Após configurar os segredos, faça um commit e push para a branch `master`
2. Acesse a aba "Actions" no seu repositório GitHub
3. Você deverá ver o workflow "Deploy to Azure" em execução
4. Após a conclusão com sucesso, verifique seu aplicativo no Azure para confirmar que foi atualizado

## Solução de problemas

Se o workflow falhar, verifique:

1. Os logs de execução na aba "Actions" para identificar o problema
2. Se os segredos estão configurados corretamente
3. Se o perfil de publicação está atualizado (eles podem expirar)
4. Se a versão do .NET configurada no workflow corresponde à versão do seu projeto

## Observações

- O perfil de publicação contém credenciais sensíveis. Nunca compartilhe seu conteúdo.
- Você pode ajustar o workflow conforme necessário para incluir etapas adicionais, como testes automatizados. 