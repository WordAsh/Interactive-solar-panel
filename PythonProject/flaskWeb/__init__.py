from flask import Flask
from datetime import timedelta


def create_app():
	app = Flask(__name__)
	app.config['SECRET_KEY'] = '5791628bb0b13ce0c676dfde280ba245'
	app.config['PERMANENT_SESSION_LIFETIME'] = timedelta(days=1)
	from flaskWeb.main.routes import main
	from flaskWeb.errors.handlers import errors
	app.register_blueprint(main)
	app.register_blueprint(errors)

	return app