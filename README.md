# ScrumPoker

## Sobre o Projeto

ScrumPoker é uma aplicação web desenvolvida para facilitar sessões de Planning Poker (também conhecido como Scrum Poker) em equipes que utilizam metodologias ágeis. O Planning Poker é uma técnica de estimativa colaborativa usada para avaliar o esforço ou complexidade relativa de tarefas de desenvolvimento.

A aplicação permite que membros da equipe votem simultaneamente, mantendo os votos ocultos até que todos tenham votado ou até que o facilitador decida revelar os resultados. Isso evita o viés de ancoragem (quando as estimativas de uma pessoa influenciam as de outras).

## Tecnologias Utilizadas

- **Backend**: ASP.NET Core (.NET 9.0)
- **Frontend**: HTML, JavaScript, Bootstrap
- **Comunicação em tempo real**: SignalR
- **Arquitetura**: Razor Pages

## Como Executar

1. Certifique-se de ter o .NET 9.0 SDK instalado
2. Clone o repositório
3. Navegue até a pasta do projeto
4. Execute o comando:
   ```
   dotnet run
   ```
5. Acesse a aplicação em `https://localhost:5001` ou `http://localhost:5000`

## Como Usar

### Entrar na Sessão

1. Abra a aplicação no navegador
2. Clique no botão "Entrar no Sistema"
3. Digite seu nome quando solicitado
4. Seu nome será exibido na lista de usuários conectados

### Participar da Votação

1. Quando uma história de usuário ou tarefa for apresentada para estimativa, escolha um valor clicando em um dos botões de votação
2. Os valores seguem a sequência comumente usada no Planning Poker: 0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100, além de "?" (não sei) e "☕" (pausa para café)
3. Seu voto permanecerá oculto para os outros participantes, mas você poderá ver que votou
4. Quando todos votarem ou quando o facilitador decidir, os votos podem ser revelados

### Facilitar a Sessão

Como facilitador, você pode:
1. Monitorar quem já votou (indicado por "🙈") e quem ainda não votou (indicado por "-")
2. Clicar em "Revelar Notas" para mostrar todos os votos para todos os participantes
3. Após a discussão e consenso, clicar em "Zerar Votos" para iniciar uma nova rodada

## Funcionalidades

- **Entrada e Saída de Usuários**: Os usuários podem entrar e sair da sessão a qualquer momento
- **Persistência de Nome**: O nome do usuário é salvo no localStorage do navegador
- **Votação em Tempo Real**: Os usuários podem votar simultaneamente
- **Ocultação de Votos**: Os votos permanecem ocultos até serem revelados
- **Revelação de Votos**: Os votos podem ser revelados com um único clique
- **Reinício de Votação**: A votação pode ser reiniciada para uma nova rodada
- **Atualizações em Tempo Real**: Todas as ações são refletidas instantaneamente para todos os usuários conectados
- **Reconexão Automática**: Em caso de perda de conexão, a aplicação tenta reconectar automaticamente

## Estrutura do Projeto

- **ScrumPokerHub.cs**: Implementa a lógica de comunicação em tempo real usando SignalR
- **Pages/Index.cshtml**: Contém a interface do usuário e a lógica de frontend
- **Program.cs**: Configura os serviços e o pipeline HTTP da aplicação

## Limitações Atuais

- Não há persistência de dados entre sessões do servidor
- Não existe autenticação de usuários
- Não há suporte para múltiplas salas de votação simultâneas

## Possíveis Melhorias

- Implementar salas separadas para diferentes equipes ou sessões
- Adicionar estatísticas e gráficos para análise de votação
- Integrar com ferramentas de gerenciamento de projetos (Jira, Azure DevOps, etc.)
- Adicionar recursos de chat para discussão durante a sessão 