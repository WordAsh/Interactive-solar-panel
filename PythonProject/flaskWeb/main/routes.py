from flask import render_template, Blueprint, redirect, url_for, request
from flaskWeb.main import forms
from flaskWeb.main.esp import get_altitude, get_length, get_min_length, get_next_hour_altitude
import json
import os
import time
import paho.mqtt.client as mqtt
from flaskWeb.main import utils



main = Blueprint('main', __name__)
ang1,ang2 = None, None #全局参数

@main.route("/")
@main.route("/home")
@main.route("/auto")
def home():
	global len1,len2
	alt,altitude,len1,len2,ang1,ang2 = utils.get_data()
	return render_template("home.html",alt=alt,altitude=altitude,len1=len1,len2=len2,ang1=ang1,ang2=ang2)


@main.route("/manual",methods=['GET','POST'])
def manual():
	form = forms.AngleForm()
	if form.validate_on_submit():
		global ang1,ang2 #设置全局参数
		form = request.form
		ang1 = form.get("angle1")
		ang2 = form.get("angle2")
		len1 = round(utils.compute_relative_len1(float(ang1),205),1) + 205
		len2 = round(utils.compute_relative_len2(float(ang1),float(ang2),455),1) + 455

		alt, altitude, _ , _, _, _ = utils.get_data()
		return render_template("manual_result.html", alt=alt, altitude=altitude, len1=len1, len2=len2, ang1=ang1, ang2=ang2)
	alt, altitude, len1, len2, ang1, ang2 = utils.get_data()
	return render_template("manual.html",alt=alt,altitude=altitude,len1=len1,len2=len2,ang1=ang1,ang2=ang2,form=form)


@main.route("/manual/result",methods=["GET","POST"])
def manual_result():
	if request.method == 'POST':
		form = request.form
		ang1 = form.get("angle1")
		ang2 = form.get("angle2")
		len1 = round(utils.compute_relative_len1(float(ang1),205),1) + 205
		len2 = round(utils.compute_relative_len2(float(ang1),float(ang2),455),1) + 455
		alt, altitude, _, _, _, _ = utils.get_data()
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


def sendEsp():
	# 导入json文件
	path_to_json = '/home/rigon/Projects/SunLightRef/data'
	json_files = [pos_json for pos_json in os.listdir(path_to_json) if pos_json.endswith('.json')]
	for file in json_files:
		with open(os.path.join(path_to_json, file), 'r') as f:
			if str(file)[0] == 'a':
				altitude_dict = json.load(f)
			else:
				length_dict = json.load(f)

	broker = "127.0.0.1"
	port = 1883
	keepalive = 60
	topic = "rod_length/msg"
	client = mqtt.Client()
	client.connect(broker, port)

	while True:
		global ang1,ang2
		if ang1 and ang2:
			len1 = round(utils.compute_relative_len1(float(ang1), 205),1)
			len2 = round(utils.compute_relative_len2(float(ang1), float(ang2), 455),1)
			length = (float(len1) ,float(len2))
		else:
			altitude = get_altitude(altitude_dict)
			next_hour_alt = get_next_hour_altitude(altitude_dict)
			length1 = get_length(length_dict, altitude) #获取整点长度
			length2 = get_length(length_dict,next_hour_alt) #下一整点长度
			length = get_min_length(length1,length2)  #当前长度

		msg = f"{length}"
		result = client.publish(topic, msg)
		status = result[0]
		if status == 0:
			print(f"Send '{msg}' to topic '{topic}'")
		else:
			print(f"Failed to send message to topic {topic}")

		time.sleep(10)