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
    - [UserRole](#userrole)
    - [DocumentType](#documenttype)
    - [DocumentProcessingStatus](#documentprocessingstatus)

## Base URL
`http://localhost:5000/api` (or your production url)

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
    "role": "Patient" // "Patient" or "Doctor"
  }
  ```
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "User Registered Successfully",
      "data": "User Registered Successfully",
      "errors": []
    }
    ```

### Login
Authenticates a user and returns a JWT token.

- **URL**: `/login`
- **Method**: `POST`
- **Auth**: None
- **Request Body**:
  ```json
  {
    "email": "john@example.com",
    "password": "Password123!"
  }
  ```
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "Login Successful",
      "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
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
    "newPassword": "NewPassword123!"
  }
  ```
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "Password Reset Successfully",
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
      "message": "Document uploaded successfully.",
      "data": {
        "id": 1,
        "fileName": "report_pdf"
      },
      "errors": []
    }
    ```

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
      "message": "Profile retrieved successfully",
      "data": {
        "id": "e98...",
        "fullName": "John Doe",
        "email": "john@example.com",
        "role": "Patient"
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
    "phoneNumber": "12345678"
  }
  ```
- **Response**:
  - `200 OK`:
    ```json
    {
      "success": true,
      "message": "Profile updated successfully.",
      "data": null,
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
      "message": "Account deactivated. You have 30 days to recover it.",
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
      "message": "Hard delete request received. Your account will be removed permanently.",
      "data": null,
      "errors": []
    }
    ```

---

## 4. Enums

### UserRole
- `Patient`
- `Doctor`
- `Admin`

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
