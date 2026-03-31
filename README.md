# SehhaLink API Documentation

This document provides a detailed reference for the backend APIs available in SehhaLink (Sehha360).

## Table of Contents
- [SehhaLink API Documentation](#sehhalink-api-documentation)
  - [Table of Contents](#table-of-contents)
  - [Base URL](#base-url)
  - [Authentication](#authentication)
  - [1. Account APIs](#1-account-apis)
    - [Register User](#register-user)
    - [Login](#login)
    - [Forgot Password](#forgot-password)
    - [Reset Password](#reset-password)
  - [2. Document APIs](#2-document-apis)
    - [Upload Document](#upload-document)
    - [Get Document URL](#get-document-url)
  - [3. User Profile APIs](#3-user-profile-apis)
    - [Get My Profile](#get-my-profile)
    - [Update My Profile](#update-my-profile)
    - [Deactivate Account](#deactivate-account)
    - [Hard Delete Account](#hard-delete-account)
  - [4. Enums](#4-enums)
    - [Gender](#gender)
    - [UserRole](#userrole)
    - [DocumentType](#documenttype)
    - [DocumentProcessingStatus](#documentprocessingstatus)

## Base URL
`http://localhost:5000/api` (or your production url)

## Environment Variables
Ensure the following variables are set in your `.env` file before running:
- `OCR_SPACE_API_KEY`: API key for OCR.Space (optional fallback).
- `PADDLE_OCR_URL`: URL for the PaddleOCR layout parsing service.
- `PADDLE_OCR_TOKEN`: Authorization token for the PaddleOCR service.
- `GROQ_API_KEY`: API key for Groq AI to generate patient-friendly medical summaries.

## Authentication
Protected endpoints require authentication via JWT Bearer Token.
- **Header**: `Authorization: Bearer <token>`

---

## 1. Account APIs
Base Path: `/api/Auth`

### Register User
Creates a new Patient or Doctor account.

- **URL**: `/register`
- **Method**: `POST`
- **Auth**: None
- **Request Body**:
  ```json
  {
    "fullName": "John Doe",
    "email": "john@example.com",
    "password": "Password123!",
    "confirmPassword": "Password123!",
    "birthDate": "1990-01-01",
    "gender": "Male",
    "phoneNumber": "1234567890",
    "role": "Patient"
  }
  ```
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "User Registered Successfully",
      "data": null,
      "errors": []
    }
    ```

### Login
Authenticates a user and returns a JWT token along with profile information.

- **URL**: `/login`
- **Method**: `POST`
- **Auth**: None
- **Request Body**:
  ```json
  {
    "email": "john@example.com",
    "password": "Password123!",
    "rememberMe": false
  }
  ```
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "Login Successful",
      "data": {
        "id": "123e4567-e89b-12d3...",
        "fullName": "John Doe",
        "email": "john@example.com",
        "birthDate": "1990-01-01",
        "gender": "Male",
        "createdAt": "2023-10-27T10:15:00Z",
        "role": "Patient",
        "phoneNumber": "1234567890",
        "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6..."
      },
      "errors": []
    }
    ```

### Forgot Password
Initiates the password reset process by sending an OTP to the user's email.

- **URL**: `/forgot-password`
- **Method**: `POST`
- **Auth**: None
- **Request Body**:
  ```json
  {
    "email": "john@example.com"
  }
  ```
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "If an account with that email exists, a password reset code has been sent.",
      "data": null,
      "errors": []
    }
    ```

### Reset Password
Verifies the OTP and resets the user's password.

- **URL**: `/reset-password`
- **Method**: `POST`
- **Auth**: None
- **Request Body**:
  ```json
  {
    "email": "john@example.com",
    "otp": "123456",
    "newPassword": "NewPassword123!",
    "confirmPassword": "NewPassword123!"
  }
  ```
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "Password has been reset successfully. You can now log in with your new password.",
      "data": null,
      "errors": []
    }
    ```

---

## 2. Document APIs
Base Path: `/api/Documents`

### Upload Document
Uploads a new medical document for the authenticated patient.

- **URL**: `/upload`
- **Method**: `POST`
- **Auth**: Required
- **Request Body**: Multipart form data with a single `file`.
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "Document uploaded successfully and is being processed in the background. Check back shortly for the summary.",
      "data": {
        "id": 1,
        "fileName": "report_pdf"
      },
      "errors": []
    }
    ```
    > **Note:** The document text extraction (OCR) and AI medical summarization (Groq) are processed asynchronously to ensure fast upload response times. The `ExtractedText`, `PatientSummary`, and `ProcessingStatus` properties on the backend will automatically update once the background tasks complete.

### Get Document URL
Generates a 15-minute secure signed URL for a specific document.

- **URL**: `/{id}/url`
- **Method**: `GET`
- **Auth**: Required
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "Secure URL generated successfully.",
      "data": {
        "url": "https://signed-supabase-url...",
        "expiresAt": "2023-10-27T10:15:00Z"
      },
      "errors": []
    }
    ```

---

## 3. User Profile APIs
Base Path: `/api/User`

### Get My Profile
Retrieves the logged-in user's profile details.

- **URL**: `/me`
- **Method**: `GET`
- **Auth**: Required
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "User profile retrieved successfully",
      "data": {
        "fullName": "John Doe",
        "email": "john@example.com",
        "birthDate": "1990-01-01",
        "gender": "Male",
        "createdAt": "2023-10-27T10:15:00Z",
        "role": "Patient",
        "phoneNumber": "1234567890"
      },
      "errors": []
    }
    ```

### Update My Profile
Updates the logged-in user's profile.

- **URL**: `/me`
- **Method**: `PATCH`
- **Auth**: Required
- **Request Body**:
  ```json
  {
    "fullName": "John Updated",
    "birthDate": "1990-01-01",
    "gender": "Male",
    "phoneNumber": "1234567890"
  }
  ```
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "Profile updated successfully",
      "data": {
        "fullName": "John Updated",
        "email": "john@example.com",
        "birthDate": "1990-01-01",
        "gender": "Male",
        "createdAt": "2023-10-27T10:15:00Z",
        "role": "Patient",
        "phoneNumber": "1234567890"
      },
      "errors": []
    }
    ```

### Deactivate Account
Soft-deletes the current account with a 30-day grace period.

- **URL**: `/deactivate`
- **Method**: `POST`
- **Auth**: Required
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "Account deactivated successfully. You have 30 days to reactivate your account by logging in.",
      "data": null,
      "errors": []
    }
    ```

### Hard Delete Account
Requests an immediate hard removal of all user data (GDPR compliant).

- **URL**: `/me`
- **Method**: `DELETE`
- **Auth**: Required
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "Account and all associated data have been permanently deleted.",
      "data": null,
      "errors": []
    }
    ```

---

## 4. Enums
When passing enum values in JSON payloads, use the string representations listed below, as the API uses string conversion for enums.

### Gender
- `Male`
- `Female`

### UserRole
- `Doctor`
- `Patient`

### DocumentType
- `PDF`
- `JPEG`
- `PNG`
- `DICOM`

### DocumentProcessingStatus
- `Pending`
- `Scanning`
- `Clean`
- `MalwareDetected`
- `Quarantined`
- `Processing`
- `Processed`
