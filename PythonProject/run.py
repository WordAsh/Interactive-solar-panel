from flaskWeb import create_app
from flaskWeb.main.routes import sendEsp
import threading


def run_app():
	app = create_app()
	app.run(host='0.0.0.0', port=8080) #运行网页


if __name__ == "__main__":
	app_thread = threading.Thread(target=run_app)
	app_thread.start()
	send_thread = threading.Thread(target=sendEsp)
	send_thread.start()




