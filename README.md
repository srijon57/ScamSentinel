

# Scam-Sentinel Project

  

## Overview

This repository contains the project proposal and development files for Scam-Sentinel, a community-driven platform designed to combat fraud in Bangladesh. The project was created as part of the CSE 3200: Software Development - V course at Ahsanullah University of Science and Technology (AUST).

  

## 📝 Project Description

Scam-Sentinel aims to address the rising fraud incidents in Bangladesh by providing a platform for users to report, verify, and learn about scams. It includes features for online and offline scam reporting, community validation, and educational resources to enhance digital literacy.

  

## 💡 Project Features

i. **User Authentication and Authorization**

- Secure user sign-up/login system.

- Admin and user login system.

  

ii. **Scam/Fraud Alert Submission**

- Online Scam → Select platform (e.g., Facebook, WhatsApp, Email, E-commerce site).

- Offline Scam → Provide location (map/location field).

  

iii. **Form-based Submission**

- Fields like title, description, date/time, and scam type.

  

iv. **Option to Upload Evidence**

- Upload images/screenshots as evidence.

  

v. **Post Engagement & Validation**

- Upvote system → Victims or people who faced similar scams can upvote.

- Downvote system → Community can flag/report suspicious or fake alerts.

  

vi. **Comment System**

- Comment section for additional information, opinions, or related experiences.

  

vii. **Scam Categorization & Search**

- Categorization by type, platform, and location.

- Search and filter options (by keyword, date, location, scam type).

  

viii. **Future Enhancements**

- Integration with law enforcement or cybercrime helplines.

- AI chatbot for scam awareness Q&A.

  

### 🎯 Objectives

- **Enable Scam Investigation**: Provide a secure platform for Bangladeshi users to check scams online (e.g., via Facebook, WhatsApp, email) and offline (e.g., street fraud in Dhaka or Chittagong), with fields for title, description, date, and evidence upload.

- **Promote Community Verification**: Implement upvote/downvote and comment systems to allow users to verify scam alerts, build trust, and reduce misinformation.

- **Increase Digital Literacy**: Offer educational resources, quizzes, and an AI chatbot to teach users about scam types and prevention strategies, targeting low digital literacy in rural areas.

- **Support Victims**: Create a safe space for victims to share their experiences, join support groups, and access recovery resources, reducing stigma in communities.

- **Ensure Environmental Sustainability**: Use a digital platform to reduce paper-based awareness campaigns, in line with Bangladesh’s eco-friendly practices.

  

### 👥 Target Audience

- Citizens seeking to verify and report scams.

- Victims looking for support and recovery resources.

- Law enforcement agencies aiming to streamline scam reporting.

  

## 📜 API Endpoints

### Authentication

- **POST /api/auth/register**: User registration.

- **POST /api/auth/login**: User login.

  

### Scam Alerts

- **GET /api/scams**: Fetch all scam alerts.

- **GET /api/scams/{id}**: Fetch a specific scam alert.

- **POST /api/scams**: Create a new scam alert.

- **PUT /api/scams/{id}**: Update a scam alert.

- **DELETE /api/scams/{id}**: Delete a scam alert (Admin only).

  

### Evidence

- **POST /api/evidence**: Upload evidence for a scam alert.

- **GET /api/evidence/{id}**: Fetch evidence for a specific scam alert.

  

### Engagement

- **POST /api/votes**: Upvote or downvote a scam alert.

- **POST /api/comments**: Post a comment on a scam alert.

  

### Search

- **GET /api/search**: Search and filter scam alerts by type, platform, and location.

  

## 📝 Milestones

### Milestone 1: Initial Setup and Basic Features

- [x] Set up backend and frontend.

- [x] Implement user authentication (registration and login).

- [x]  Create API endpoints for scam alerts and evidence.

- [x] Basic UI for login, registration, and homepage.

  

### Milestone 2: Advanced Features and Interactions

- [x]  Implement scam alert submission and validation system.

- [x]  Add search and filter functionality.

- [x]  UI for scam reporting, search functionality, and viewing scam details.

- [x]  Implement evidence upload feature.

  

### Milestone 3: Final Touches and Deployment

- [x] Complete testing and bug fixes.


- [x] Deployment to web.

  

## 💻 Technologies Used

- **Backend**: ASP.NET Core MVC

- **Frontend**: cshtml blazor with tailwindcss

- **Database**: PostgresSQL

- **Version Control**: Git

- **Repository**: GitHub

- **Rendering Method**: Client-Side Rendering (CSR)

  

## 🚧 Installation
### Prerequisites
- .NET SDK (ASP.NET Core MVC) >= 6.0
- MySQL/PostgreSQL
- Visual Studio or VS Code with C# extension

### Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/scam-sentinel.git
   ```
2. Navigate to the project directory:
   ```bash
   cd scam-sentinel
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Update the `appsettings.json` with your MySQL/PostgreSQL connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=scam_sentinel;User=your_user;Password=your_password;"
     }
   }
   ```
5. Apply migrations:
   ```bash
   dotnet ef database update
   ```
   
6. Add TailwindCSS:
   ```bash
   npm install
   ```
   
7. Run Tailwindcss:
   ```bash
   npm run build:css
   ```
8. Run the application:
   ```bash
   dotnet run
   ```



  

## 👷 Team Members <a id="team"></a>

| **ID**       | **Name**                       | **Email**                          | **Github**                          | **Role**             |
|--------------|--------------------------------|------------------------------------|-------------------------------------|----------------------|
| 20220104120  | **Zawad Al Mahi**             | zawadalmahi@gmail.com             | [zawadalmahi](https://github.com/zawadalmahi) | Frontend + Backend  |
| 20220104123  | **Abdullah Al Jubayer**       | abdullahaljubair2019@gmail.com    | [abduillahaljubair](https://github.com/abduillahaljubair) | Frontend + Backend |
| 20220104124  | **KM Hasibur Rahman Srijon**  | srijond57@gmail.com               | [srijon57](https://github.com/srijon57) | Lead                |

  

## ✔️ Live Project & Mock UI

**Mock UI Link**: [Figma](https://www.figma.com/design/84CfcD5h4SSnipXPLE0kRG/ScamAlert?node-id=0-1&t=iiAMeTWo0BlKJ5Qk-1)

**Live Project Link**: [Live](http://scansentinel.runasp.net)

  

`Thank you for supporting us.`
