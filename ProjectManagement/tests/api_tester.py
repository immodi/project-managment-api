import json
import urllib.request
import urllib.error

BASE_URL = "http://localhost:5094"


# ---------- HTTP helper ----------
def request(method, path, token=None, body=None):
    url = BASE_URL + path

    data = None
    if body is not None:
        data = json.dumps(body).encode("utf-8")

    req = urllib.request.Request(url, data=data, method=method)
    req.add_header("Content-Type", "application/json")

    if token:
        req.add_header("Authorization", f"Bearer {token}")

    try:
        with urllib.request.urlopen(req) as res:
            raw = res.read().decode()

            print("\n==============================")
            print(f"{method} {path}")
            print("------------------------------")
            print(raw)
            print("==============================")

            return json.loads(raw) if raw else None

    except urllib.error.HTTPError as e:
        err_body = e.read().decode()

        print("\n==============================")
        print(f"❌ ERROR {e.code} {path}")
        print("------------------------------")
        print(err_body)
        print("==============================")

        return None


# ---------- Auth ----------
def register():
    print("\n== Register ==")
    return request("POST", "/api/auth/register", body={
        "email": "test@example.com",
        "password": "Test1234A"
    })


def login():
    print("\n== Login ==")
    res = request("POST", "/api/auth/login", body={
        "email": "test@example.com",
        "password": "Test1234A"
    })

    if res and "data" in res:
        return res["data"]["token"]

    return None


# ---------- Projects ----------
def create_project(token):
    print("\n== Create Project ==")
    res = request("POST", "/api/projects", token, {
        "name": "Test Project",
        "description": "Created by script"
    })

    if res:
        return res["data"]["id"]
    return None


def get_projects(token):
    print("\n== Get Projects ==")
    return request("GET", "/api/projects", token)


# ---------- Tasks ----------
def create_task(token, project_id):
    print("\n== Create Task ==")
    return request("POST", "/api/tasks", token, {
        "title": "Test Task",
        "description": "Generated via script",
        "dueDate": "2099-01-01T00:00:00Z",
        "priority": 1,
        "projectId": project_id
    })


def get_tasks(token, project_id):
    print("\n== Get Tasks ==")
    return request("GET", f"/api/tasks/project/{project_id}", token)


# ---------- main ----------
def main():
    register()

    token = login()
    if not token:
        print("❌ Login failed")
        return

    print("\nJWT (short):", token[:40], "...")

    project_id = create_project(token)
    if not project_id:
        print("❌ Project creation failed")
        return

    print("\nProject ID:", project_id)

    get_projects(token)
    create_task(token, project_id)
    get_tasks(token, project_id)


if __name__ == "__main__":
    main()
