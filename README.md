# Student API Documentation

This document provides a detailed overview of the Student REST API, which is responsible for managing student data.

## 1. Base URL

All API endpoints are relative to the following base URL:

`{{base_url}}/students`

## 2. Entities and Relationships

### 2.1. Student

The core entity of this API. It represents a student enrolled in the institution.

| Field               | Type               | Description                                                           | Constraints      |
| ------------------- | ------------------ | --------------------------------------------------------------------- | ---------------- |
| `_id`               | UUID               | The unique identifier for the student (primary key).                  | Required         |
| `name`              | String             | The full name of the student.                                         | Required         |
| `enrollment`        | String             | The student's unique enrollment ID.                                   | Required, Unique |
| `email`             | String             | The student's primary email address.                                  | Required, Unique |
| `course_curriculum` | String             | The curriculum code or name the student is enrolled in.               | Required         |
| `phoneNumbers`      | Array[PhoneNumber] | A list of the student's phone numbers. This is a subdocument.         | Optional         |
| `classes`           | Array[UUID]        | An array of UUIDs referencing the classes the student is enrolled in. | Required, min 1  |

### 2.2. PhoneNumber (Subdocument)

Represents a student's phone number. It is always embedded within a `Student` document.

| Field         | Type    | Description                                          | Constraints |
| ------------- | ------- | ---------------------------------------------------- | ----------- |
| `ddd`         | Integer | The area code (e.g., 51 for Porto Alegre).           | Required    |
| `number`      | Integer | The phone number.                                    | Required    |
| `description` | String  | A brief description for the number (e.g., 'Mobile'). | Optional    |

### 2.3. Class (External Reference)

The `Class` entity is managed by a separate API (`{{base_url}}/classes`). The Student API only stores an array of UUIDs to establish the relationship, indicating which classes a student is enrolled in.

- **Relationship**: A student must be enrolled in at least one class (`1..*`).

## 3. API Endpoints

The API provides standard CRUD (Create, Read, Update, Delete) operations for the `Student` entity.

### POST /students

- **Description**: Creates a new student record.
- **Request Body**: `application/json`
  - Schema: `student-schemas.json#/createStudentRequest`
  - Example: `student-examples.json#/createStudentRequestExample`
- **Responses**:
  - `201 Created`: The student was created successfully. The response body contains the newly created student object.
    - Schema: `student-schemas.json#/studentResponse`
  - `400 Bad Request`: The request body is invalid or missing required fields.

### GET /students

- **Description**: Retrieves a list of all students.
- **Responses**:
  - `200 OK`: Successfully retrieved the list of students. The response body is an array of student objects.
    - Schema: `array` of `student-schemas.json#/studentResponse`

### GET /students/{id}

- **Description**: Retrieves a single student by their unique ID.
- **URL Parameters**:
  - `id` (UUID, required): The ID of the student to retrieve.
- **Responses**:
  - `200 OK`: The requested student was found. The response body contains the student object.
    - Schema: `student-schemas.json#/studentResponse`
  - `404 Not Found`: No student with the specified ID was found.

### PUT /students/{id}

- **Description**: Updates an existing student's information. The request body should contain only the fields to be updated.
- **URL Parameters**:
  - `id` (UUID, required): The ID of the student to update.
- **Request Body**: `application/json`
  - Schema: `student-schemas.json#/updateStudentRequest`
  - Example: `student-examples.json#/updateStudentRequestExample`
- **Responses**:
  - `200 OK`: The student was updated successfully. The response body contains the updated student object.
    - Schema: `student-schemas.json#/studentResponse`
  - `400 Bad Request`: The request body is invalid.
  - `404 Not Found`: No student with the specified ID was found.

### DELETE /students/{id}

- **Description**: Deletes a student record.
- **URL Parameters**:
  - `id` (UUID, required): The ID of the student to delete.
- **Responses**:
  - `204 No Content`: The student was deleted successfully.
  - `404 Not Found`: No student with the specified ID was found.
