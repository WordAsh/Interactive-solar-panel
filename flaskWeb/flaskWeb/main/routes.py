from flask import render_template, Blueprint, redirect, url_for, request
from flaskWeb.main import utils
from flaskWeb.main import forms


main = Blueprint('main', __name__)


@main.route("/")
@main.route("/home")
@main.route("/auto")
def home():
	alt,altitude,len1,len2,ang1,ang2 = utils.get_data()
	return render_template("home.html",alt=alt,altitude=altitude,len1=len1,len2=len2,ang1=ang1,ang2=ang2)


@main.route("/manual",methods=['GET','POST'])
def manual():
	form = forms.LengthForm()
	if form.validate_on_submit():
		form = request.form
		len1 = form.get("length1")
		len2 = form.get("length2")
		alt, altitude, _, _, ang1, ang2 = utils.get_data()
		return render_template("manual_result.html", alt=alt, altitude=altitude, len1=len1, len2=len2, ang1=ang1, ang2=ang2)
	alt, altitude, len1, len2, ang1, ang2 = utils.get_data()
	return render_template("manual.html",alt=alt,altitude=altitude,len1=len1,len2=len2,ang1=ang1,ang2=ang2,form=form)


@main.route("/manual/result",methods=["GET","POST"])
def manual_result():
	if request.method == 'POST':
		form = request.form
		len1 = form.get("length1")
		len2 = form.get("length2")
		alt, altitude, _, _, ang1, ang2 = utils.get_data()
		return render_template("manual_result.html",alt=alt,altitude=altitude,len1=len1,len2=len2,ang1=ang1,ang2=ang2)
	return render_template("home.html")
@main.route("/redirect_to_auto", methods=['POST'])
def redirect_to_auto():
	return redirect(url_for('main.home'))


@main.route("/redirect_to_manual", methods=['POST'])
def redirect_to_manual():
	return redirect(url_for('main.manual'))


@main.route("/redirect_to_manual_result",methods=['POST'])
def redirect_to_manual_result():
	return redirect(url_for("main.manual_result"))


