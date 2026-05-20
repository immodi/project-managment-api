
import json
import ssl
import urllib.request
import urllib.error
from datetime import datetime, timedelta
from typing import Any

BASE_URL = "http://localhost:5094/api"


class Colors:
    GREEN = "\033[92m"
    RED = "\033[91m"
    YELLOW = "\033[93m"
    BLUE = "\033[94m"
    CYAN = "\033[96m"
    RESET = "\033[0m"


class ApiTester:
    def __init__(self):
        self.token = None
        self.project_id = None
        self.task_id = None
        self.total_tests = 0
        self.passed_tests = 0

        self.email = f"test_{datetime.utcnow().timestamp()}@example.com"
        self.password = "Password123"

        self.ssl_context = ssl._create_unverified_context()

    def log(self, message: str, color=Colors.BLUE):
        print(f"{color}{message}{Colors.RESET}")

    def success(self, message: str):
        self.passed_tests += 1
        self.log(f"✔ PASS: {message}", Colors.GREEN)

    def fail(self, message: str):
        self.log(f"✘ FAIL: {message}", Colors.RED)

    def info(self, message: str):
        self.log(f"➜ {message}", Colors.CYAN)

    def assert_equal(self, actual: Any, expected: Any, message: str):
        self.total_tests += 1

        if actual == expected:
            self.success(message)
        else:
            self.fail(f"{message} | Expected: {expected}, Got: {actual}")

    def assert_true(self, condition: bool, message: str):
        self.total_tests += 1

        if condition:
            self.success(message)
        else:
            self.fail(message)

    def make_request(self, method: str, endpoint: str, body=None, auth=True):
        url = f"{BASE_URL}{endpoint}"

        headers = {
            "Content-Type": "application/json"
        }

        if auth and self.token:
            headers["Authorization"] = f"Bearer {self.token}"

        data = None

        if body is not None:
            data = json.dumps(body).encode("utf-8")

        request = urllib.request.Request(
            url=url,
            data=data,
            headers=headers,
            method=method
        )

        self.info(f"{method} {endpoint}")

        if body:
            print(json.dumps(body, indent=2))

        try:
            with urllib.request.urlopen(request, context=self.ssl_context) as response:
                response_body = response.read().decode("utf-8")

                parsed = None
                if response_body:
                    parsed = json.loads(response_body)

                print(f"Status: {response.status}")

                if parsed:
                    print(json.dumps(parsed, indent=2))

                return {
                    "status": response.status,
                    "body": parsed
                }

        except urllib.error.HTTPError as e:
            response_body = e.read().decode("utf-8")

            parsed = None
            if response_body:
                try:
                    parsed = json.loads(response_body)
                except:
                    parsed = response_body

            print(f"Status: {e.code}")

            if parsed:
                if isinstance(parsed, dict):
                    print(json.dumps(parsed, indent=2))
                else:
                    print(parsed)

            return {
                "status": e.code,
                "body": parsed
            }

        except Exception as e:
            self.fail(f"Unexpected exception: {str(e)}")
            return {
                "status": 0,
                "body": str(e)
            }

    def validate_api_wrapper(self, response):
        self.assert_true("success" in response, "Response contains 'success'")

    def test_register(self):
        self.log("\n=== AUTH REGISTER TESTS ===", Colors.YELLOW)

        payload = {
            "email": self.email,
            "password": self.password
        }

        response = self.make_request(
            "POST",
            "/auth/register",
            payload,
            auth=False
        )

        self.assert_equal(response["status"], 200, "Register returns 200")

        body = response["body"]

        self.validate_api_wrapper(body)
        self.assert_true(body["success"] is True, "Register success is true")
        self.assert_true("data" in body, "Register contains data")
        self.assert_true("token" in body["data"], "Register contains token")
        self.assert_true(isinstance(body["data"]["token"], str), "Token is string")
        self.assert_true(len(body["data"]["token"]) > 20, "Token length valid")

        self.token = body["data"]["token"]

    def test_register_duplicate_email(self):
        self.log("\n=== DUPLICATE REGISTER TEST ===", Colors.YELLOW)

        payload = {
            "email": self.email,
            "password": self.password
        }

        response = self.make_request(
            "POST",
            "/auth/register",
            payload,
            auth=False
        )

        self.assert_equal(response["status"], 409, "Duplicate email returns 409")
        self.assert_true("error" in response["body"], "Duplicate response contains error")

    def test_register_validation(self):
        self.log("\n=== REGISTER VALIDATION TESTS ===", Colors.YELLOW)

        invalid_payloads = [
            {
                "payload": {
                    "email": "bad-email",
                    "password": "123"
                },
                "name": "Invalid email and weak password"
            },
            {
                "payload": {
                    "email": "",
                    "password": ""
                },
                "name": "Empty email and password"
            }
        ]

        for test_case in invalid_payloads:
            response = self.make_request(
                "POST",
                "/auth/register",
                test_case["payload"],
                auth=False
            )

            self.assert_equal(
                response["status"],
                400,
                f"{test_case['name']} returns 400"
            )

            self.assert_true(
                "details" in response["body"],
                f"{test_case['name']} contains validation details"
            )

    def test_login(self):
        self.log("\n=== LOGIN TESTS ===", Colors.YELLOW)

        payload = {
            "email": self.email,
            "password": self.password
        }

        response = self.make_request(
            "POST",
            "/auth/login",
            payload,
            auth=False
        )

        self.assert_equal(response["status"], 200, "Login returns 200")

        body = response["body"]

        self.assert_true(body["success"] is True, "Login success true")
        self.assert_true("token" in body["data"], "Login contains token")

    def test_login_invalid_credentials(self):
        self.log("\n=== INVALID LOGIN TEST ===", Colors.YELLOW)

        payload = {
            "email": self.email,
            "password": "WrongPassword123"
        }

        response = self.make_request(
            "POST",
            "/auth/login",
            payload,
            auth=False
        )

        self.assert_equal(response["status"], 401, "Invalid login returns 401")
        self.assert_true("error" in response["body"], "Invalid login contains error")

    def test_unauthorized_access(self):
        self.log("\n=== UNAUTHORIZED ACCESS TESTS ===", Colors.YELLOW)

        response = self.make_request(
            "GET",
            "/projects",
            auth=False
        )

        self.assert_equal(response["status"], 401, "Unauthorized projects access returns 401")

    def test_create_project(self):
        self.log("\n=== CREATE PROJECT TESTS ===", Colors.YELLOW)

        payload = {
            "name": "Backend Assessment Project",
            "description": "Testing full project flow"
        }

        response = self.make_request(
            "POST",
            "/projects",
            payload
        )

        self.assert_equal(response["status"], 200, "Create project returns 200")

        body = response["body"]

        self.assert_true(body["success"] is True, "Create project success true")
        self.assert_true("data" in body, "Project response contains data")

        project = body["data"]

        required_fields = [
            "id",
            "name",
            "description",
            "createdAt"
        ]

        for field in required_fields:
            self.assert_true(field in project, f"Project contains field '{field}'")

        self.assert_equal(project["name"], payload["name"], "Project name matches")
        self.assert_equal(project["description"], payload["description"], "Project description matches")

        self.project_id = project["id"]

    def test_create_project_validation(self):
        self.log("\n=== PROJECT VALIDATION TESTS ===", Colors.YELLOW)

        payload = {
            "name": "",
            "description": "x" * 600
        }

        response = self.make_request(
            "POST",
            "/projects",
            payload
        )

        self.assert_equal(response["status"], 400, "Invalid project returns 400")
        self.assert_true("details" in response["body"], "Project validation contains details")

    def test_get_all_projects(self):
        self.log("\n=== GET ALL PROJECTS TESTS ===", Colors.YELLOW)

        response = self.make_request(
            "GET",
            "/projects"
        )

        self.assert_equal(response["status"], 200, "Get all projects returns 200")

        body = response["body"]

        self.assert_true(isinstance(body["data"], list), "Projects data is list")
        self.assert_true(len(body["data"]) >= 1, "Projects list not empty")

    def test_get_project_by_id(self):
        self.log("\n=== GET PROJECT BY ID TESTS ===", Colors.YELLOW)

        response = self.make_request(
            "GET",
            f"/projects/{self.project_id}"
        )

        self.assert_equal(response["status"], 200, "Get project by id returns 200")

        body = response["body"]
        project = body["data"]

        self.assert_equal(project["id"], self.project_id, "Fetched project id matches")

    def test_get_project_not_found(self):
        self.log("\n=== PROJECT NOT FOUND TESTS ===", Colors.YELLOW)

        fake_id = "11111111-1111-1111-1111-111111111111"

        response = self.make_request(
            "GET",
            f"/projects/{fake_id}"
        )

        self.assert_equal(response["status"], 404, "Missing project returns 404")

    def test_update_project(self):
        self.log("\n=== UPDATE PROJECT TESTS ===", Colors.YELLOW)

        payload = {
            "name": "Updated Project Name",
            "description": "Updated project description"
        }

        response = self.make_request(
            "PUT",
            f"/projects/{self.project_id}",
            payload
        )

        self.assert_equal(response["status"], 204, "Update project returns 204")

        verify = self.make_request(
            "GET",
            f"/projects/{self.project_id}"
        )

        project = verify["body"]["data"]

        self.assert_equal(project["name"], payload["name"], "Updated project name persisted")

    def test_create_task(self):
        self.log("\n=== CREATE TASK TESTS ===", Colors.YELLOW)

        payload = {
            "title": "Implement Authentication",
            "description": "Build JWT auth flow",
            "dueDate": (datetime.utcnow() + timedelta(days=7)).isoformat(),
            "priority": 2,
            "projectId": self.project_id
        }

        response = self.make_request(
            "POST",
            "/tasks",
            payload
        )

        self.assert_equal(response["status"], 200, "Create task returns 200")

        body = response["body"]
        task = body["data"]

        required_fields = [
            "id",
            "title",
            "description",
            "status",
            "priority",
            "dueDate",
            "projectId"
        ]

        for field in required_fields:
            self.assert_true(field in task, f"Task contains field '{field}'")

        self.assert_equal(task["title"], payload["title"], "Task title matches")
        self.assert_equal(task["projectId"], self.project_id, "Task project id matches")

        self.task_id = task["id"]

    def test_create_task_validation(self):
        self.log("\n=== TASK VALIDATION TESTS ===", Colors.YELLOW)

        payload = {
            "title": "",
            "description": "x" * 1500,
            "dueDate": (datetime.utcnow() - timedelta(days=1)).isoformat(),
            "priority": 1,
            "projectId": self.project_id
        }

        response = self.make_request(
            "POST",
            "/tasks",
            payload
        )

        self.assert_equal(response["status"], 400, "Invalid task returns 400")
        self.assert_true("details" in response["body"], "Task validation contains details")

    def test_get_tasks_by_project(self):
        self.log("\n=== GET TASKS BY PROJECT TESTS ===", Colors.YELLOW)

        response = self.make_request(
            "GET",
            f"/tasks/project/{self.project_id}"
        )

        self.assert_equal(response["status"], 200, "Get tasks returns 200")

        body = response["body"]

        self.assert_true(isinstance(body["data"], list), "Tasks data is list")
        self.assert_true(len(body["data"]) >= 1, "Tasks list not empty")

    def test_update_task(self):
        self.log("\n=== UPDATE TASK TESTS ===", Colors.YELLOW)

        payload = {
            "title": "Updated Task Title",
            "description": "Updated task description",
            "dueDate": (datetime.utcnow() + timedelta(days=14)).isoformat(),
            "priority": 1
        }

        response = self.make_request(
            "PUT",
            f"/tasks/{self.task_id}",
            payload
        )

        self.assert_equal(response["status"], 200, "Update task returns 200")

        body = response["body"]

        self.assert_true(body["success"] is True, "Update task success true")

    def test_update_task_status(self):
        self.log("\n=== UPDATE TASK STATUS TESTS ===", Colors.YELLOW)

        payload = {
            "status": 1
        }

        response = self.make_request(
            "PATCH",
            f"/tasks/{self.task_id}/status",
            payload
        )

        self.assert_equal(response["status"], 200, "Update task status returns 200")

        body = response["body"]

        self.assert_true(body["success"] is True, "Update task status success true")

    def test_delete_task(self):
        self.log("\n=== DELETE TASK TESTS ===", Colors.YELLOW)

        response = self.make_request(
            "DELETE",
            f"/tasks/{self.task_id}"
        )

        self.assert_equal(response["status"], 200, "Delete task returns 200")

        body = response["body"]

        self.assert_true(body["success"] is True, "Delete task success true")

    def test_delete_project(self):
        self.log("\n=== DELETE PROJECT TESTS ===", Colors.YELLOW)

        response = self.make_request(
            "DELETE",
            f"/projects/{self.project_id}"
        )

        self.assert_equal(response["status"], 204, "Delete project returns 204")

        verify = self.make_request(
            "GET",
            f"/projects/{self.project_id}"
        )

        self.assert_equal(verify["status"], 404, "Deleted project no longer exists")

    def run(self):
        self.log("\n====================================", Colors.CYAN)
        self.log("PROJECT MANAGEMENT API FULL TEST", Colors.CYAN)
        self.log("====================================\n", Colors.CYAN)

        self.test_register_validation()
        self.test_register()
        self.test_register_duplicate_email()

        self.test_login()
        self.test_login_invalid_credentials()

        self.test_unauthorized_access()

        self.test_create_project_validation()
        self.test_create_project()
        self.test_get_all_projects()
        self.test_get_project_by_id()
        self.test_get_project_not_found()
        self.test_update_project()

        self.test_create_task_validation()
        self.test_create_task()
        self.test_get_tasks_by_project()
        self.test_update_task()
        self.test_update_task_status()
        self.test_delete_task()

        self.test_delete_project()

        self.log("\n====================================", Colors.CYAN)
        self.log("FINAL RESULTS", Colors.CYAN)
        self.log("====================================", Colors.CYAN)

        print(f"Passed: {self.passed_tests}/{self.total_tests}")

        if self.passed_tests == self.total_tests:
            self.log("\nALL TESTS PASSED ✔", Colors.GREEN)
        else:
            self.log("\nSOME TESTS FAILED ✘", Colors.RED)


if __name__ == "__main__":
    tester = ApiTester()
    tester.run()
