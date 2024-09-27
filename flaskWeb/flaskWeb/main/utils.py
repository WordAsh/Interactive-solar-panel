import json
from datetime import datetime


def get_altitude(altitude_data):
    # 获取当前时刻太阳高度角
    begin_hour = 7
    current_time = datetime.now()
    month, day, hour = current_time.month, current_time.day, current_time.hour
    if hour < 7:
        hour = 7
    elif hour > 17:
        hour = 17

    altitude = altitude_data[f"{month - 1}"][f"{day - 1}"][hour - begin_hour - 1]
    return int(altitude)


def get_next_hour_altitude(altitude_data):
    # 获取下一时刻太阳高度角
    begin_hour = 7
    current_time = datetime.now()
    month, day, hour = current_time.month, current_time.day, current_time.hour
    next_hour = hour + 1
    if next_hour < 7:
        next_hour = 7
    elif next_hour > 17:
        next_hour = 17

    altitude = altitude_data[f"{month - 1}"][f"{day - 1}"][next_hour - begin_hour - 1]
    return int(altitude)


def get_length(length_data,altitude):
    # 取给定太阳高度角的伸缩杆长度
    rod1_base_length = 205
    rod2_base_length = 455
    if altitude < 34 or altitude > 81:
        return rod1_base_length,rod2_base_length
    else:
        rod1_length, rod2_length = length_data[f"{altitude}"]
        return rod1_length,rod2_length


def get_min_value(value1,value2):
    #计算每10分钟的数据
    delta_time = 10
    time_count = 6 #每小时分6段，10分钟一段
    pre1, pre2 = value1
    aft1, aft2 = value2
    delta1 = (float(aft1) - float(pre1)) / time_count
    delta2 = (float(aft2) - float(pre2)) / time_count

    curr_time = datetime.now()
    minute = curr_time.minute
    val1 = (minute // delta_time + 1) * delta1 + float(pre1)
    val2 = (minute // delta_time + 1) * delta2 + float(pre2)
    return round(val1,1),round(val2,1)


def get_data():
	#获取table数据
	with open("flaskWeb/static/altitude.json","r") as f:
		altitude_dict = json.load(f)

	with open("flaskWeb/static/length.json","r") as f:
		length_dict = json.load(f)

	with open("flaskWeb/static/webdata.json","r") as f:
		datas = json.load(f)

	altitude = get_altitude(altitude_dict)
	next_hour_alt = get_next_hour_altitude(altitude_dict)
	length1 = get_length(length_dict, altitude) #获取整点长度
	length2 = get_length(length_dict,next_hour_alt) #下一整点长度
	len1,len2 = get_min_value(length1,length2)  #当前长度

	if int(altitude) < 34:
		alt = 34
	elif int(altitude) > 81:
		alt = 81
	else:
		alt = int(altitude)
	if int(next_hour_alt) < 34:
		next_hour_alt = 34

	angle1 = [float(x) for x in datas[str(alt)][2:]] #整点角度
	angle2 = [float(x) for x in datas[str(next_hour_alt)][2:]] #下一整点角度
	ang1,ang2 = get_min_value(angle1,angle2) #当前角度

	return str(alt),altitude,len1,len2,ang1,ang2