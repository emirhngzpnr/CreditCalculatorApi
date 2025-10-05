Credit Calculator API — Full Stack Project



This project was developed by Emirhan Efe Gözpınar during his internship at VakıfBank.



&nbsp;Overview



Credit Calculator API is a full-stack financial application that provides a secure, scalable, and real-time credit application and management system.



Users can:



Register securely with JWT and AES-256 encryption.



Verify their accounts through email verification.



Perform credit installment (itfa) calculations.



Apply to become a customer of a bank.



View and apply for bank campaigns filtered by bank and credit type.



Administrators can:



Manage users, banks, and campaigns from a centralized admin panel.



Approve, reject, or update credit and membership applications.



Monitor system health and application metrics through Prometheus and Grafana alerts.



The system:



Uses Kafka for asynchronous risk evaluation and real-time status updates.



Continuously logs activities and performance metrics.



Ensures sensitive data is securely encrypted and auditable.



&nbsp;Folder Structure

staj-proje/

├── CreditCalculatorApi/     # Backend (.NET Core)

├── Frontend/                # Angular 20 (Standalone)

├── monitoring/              # Prometheus, Grafana, Kafka Exporters

├── .gitignore

├── README.md

└── appsettings.example.json



&nbsp;System Architecture



Backend (.NET Core) → Provides RESTful APIs, handles authentication, encryption, and business logic.



Frontend (Angular 20) → Offers a modern UI for users and admins.



Kafka → Manages event-driven communication for risk evaluation and logging.



Prometheus + Grafana → Collects and visualizes application metrics with alerting mechanisms.



Databases → MSSQL (primary) and MongoDB (read model + logs).



&nbsp;How to Run

&nbsp;Backend (.NET Core)

cd CreditCalculatorApi

dotnet restore

dotnet build

dotnet run



💻 Frontend (Angular 20+)

cd Frontend

npm install

npm start



&nbsp;Monitoring Stack (Docker)

cd monitoring

docker-compose up -d



&nbsp;Key Features



* &nbsp;Secure Authentication: JWT-based login with AES-256 data encryption.
  
* &nbsp; Email Verification: Users must confirm their accounts before accessing the system.
  
* &nbsp;Credit Calculation: Dynamic credit payment (itfa) table generation.
  
* &nbsp;Bank Membership \& Campaign Applications: Users can apply to become customers and join campaigns.
  
* &nbsp;Kafka-based Risk Analysis: Async microservice communication to evaluate risk and update status.
  
* &nbsp;Monitoring \& Alerting: Prometheus + Grafana dashboards for metrics and alerting rules.
  
* &nbsp; Admin Management: Full CRUD control over users, banks and campaigns.





&nbsp;Technologies

| Category           | Stack                                       |

| ------------------ | ------------------------------------------- |

| \*\*Backend\*\*        | .NET 8, C#, EF Core, FluentValidation       |

| \*\*Frontend\*\*       | Angular 20, Bootstrap                       |

| \*\*Monitoring\*\*     | Prometheus, Grafana, Kafka Exporter         |

| \*\*Database\*\*       | MSSQL, MongoDB                              |

| \*\*Messaging\*\*      | Kafka                                       |

| \*\*Logging\*\*        | Kafka → MongoDB → Grafana                   |

| \*\*PDF Generation\*\* | DinkToPdf                                   |

| \*\*Security\*\*       | JWT, AES-256 Encryption, Email Verification |



&nbsp;Author



Emirhan Efe Gözpınar

Trainee Software Engineer — VakıfBank 

&nbsp;Istanbul, Türkiye

&nbsp;July – September 2025



💬 Summary



This project demonstrates the design and implementation of a secure, observable, and event-driven credit management system built with enterprise-grade technologies.

It integrates modern DevOps tools for real-time monitoring and follows professional architecture patterns for scalability and maintainability.



&nbsp;Notes:

Sensitive configuration files (appsettings.json, .env) are excluded from the repository for security.

You can find configuration examples inside appsettings.example.json.

