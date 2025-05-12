# Sistema de Agendamentos

## Identificação da Aplicação
Sistema web para gerenciamento de agendamentos, clientes, funcionários, serviços e relatórios. A aplicação oferece funcionalidades para diferentes perfis de usuários, como administradores, funcionários e clientes, permitindo o controle eficiente das operações de agendamento.

## Tecnologias Utilizadas
- **Backend:**
  - ASP.NET Core MVC
  - Entity Framework Core
  - Microsoft Identity (autenticação e autorização)
  - SQL Server (banco de dados)

- **Frontend:**
  - Tailwind CSS
  - Font Awesome
  - jQuery
  - HTML5/CSS3
  - JavaScript

## Objetivo da Aplicação
O objetivo principal da aplicação é facilitar o gerenciamento de agendamentos em uma organização, permitindo que clientes possam marcar serviços, funcionários possam visualizar seus agendamentos, e administradores possam gerenciar usuários, cargos, serviços e gerar relatórios detalhados sobre o desempenho e faturamento.

### Funcionalidades Principais

#### Área do Cliente
- Cadastro e login de usuários
- Agendamento de serviços
- Visualização de histórico de agendamentos
- Gerenciamento de perfil

#### Área do Funcionário
- Visualização de agendamentos
- Gerenciamento de perfil
- Acompanhamento de serviços

#### Área Administrativa
- Gestão de funcionários
- Gestão de clientes
- Gestão de serviços
- Gestão de cargos
- Relatórios:
  - Agendamentos
  - Desempenho de funcionários
  - Faturamento diário

## Configuração do Ambiente

### Pré-requisitos
- .NET 6.0 ou superior
- SQL Server
- Node.js (para desenvolvimento frontend)

### Instalação
1. Clone o repositório
2. Restaure os pacotes NuGet
3. Configure a string de conexão no arquivo `appsettings.json`
4. Execute as migrações do Entity Framework
5. Execute a aplicação

```bash
# Restaurar pacotes
dotnet restore

# Aplicar migrações
dotnet ef database update

# Executar aplicação
dotnet run
```

## Estrutura do Projeto
- `Controllers/`: Controladores MVC
- `Models/`: Models e DTOs
- `Views/`: Views do MVC
- `Entities/`: Entidades do domínio
- `Infrastructure/`: Configurações e serviços
- `Interfaces/`: Interfaces dos serviços

## Segurança
A aplicação utiliza o Identity Framework para autenticação e autorização, garantindo:
- Autenticação segura de usuários
- Autorização baseada em roles
- Proteção contra CSRF
- Senhas hasheadas
