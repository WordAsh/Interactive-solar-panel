import json
import math
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

def compute_relative_len1(angle,base_length):
    '''
    angle为反射板旋转角度
    M为反射板1边缘点
    P为板上铰接点
    Q为杆上铰接点
    O为杆固定点
    以O为原点建立坐标系
    M点坐标为（26.648，156）
    限制angle角度最大为30°
    '''
    theta = angle / 180 * math.pi
    coord_M = (26.648,156)
    PM = 200
    QP = 46
    x_Q = coord_M[0] - PM * math.cos(theta) - QP * math.cos(math.pi / 2 - theta)
    y_Q = coord_M[1] + PM * math.sin(theta) - QP * math.sin(math.pi / 2 - theta)
    OQ = math.sqrt(x_Q ** 2 + y_Q ** 2)
    return OQ - base_length


def compute_relative_len2(angle1,angle2,base_length2):
    '''
    以伸缩杆1固定点为原点建立坐标系
    则伸缩杆2的固定点坐标为(-650，0)
    反射板1的端点与反射板2的端点近似看作高度相同，距离差30
    先计算得到通过反射板1的牵引反射板2达到水平
    再利用计算反射板1的思路计算一遍
    '''
    panel_length = 800
    PQ = 46
    PM2 = 291.2
    coord_O = (-650,0)
    theta1 = angle1 / 180 * math.pi
    coord_M1 = (26.648,156)
    coord_M2 = (coord_M1[0] - panel_length * math.cos(theta1) - 30, coord_M1[1] + panel_length * math.sin(theta1))


    theta2 = angle2 / 180 * math.pi
    x_Q2 = coord_M2[0] - PM2 * math.cos(theta2) - PQ * math.cos(math.pi / 2 - theta2)
    y_Q2= coord_M2[1] + PM2 * math.sin(theta2) - PQ * math.sin(math.pi / 2 - theta2)
    OQ2 = math.sqrt((x_Q2 - coord_O[0]) ** 2 + y_Q2 ** 2)
    return OQ2 - base_length2