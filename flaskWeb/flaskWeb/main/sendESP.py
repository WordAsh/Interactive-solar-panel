import json
import os
import time
from datetime import datetime
import paho.mqtt.client as mqtt

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
        return 0,0
    else:
        rod1_length, rod2_length = length_data[f"{altitude}"]
        diff1 = float(rod1_length) - rod1_base_length
        diff2 = float(rod2_length) - rod2_base_length
        return diff1,diff2


def get_min_length(length1,length2):
    #计算每10分钟的伸缩杆相对长度
    delta_time = 10
    time_count = 6 #每小时分6段，10分钟一段
    pre1, pre2 = length1
    aft1, aft2 = length2
    delta1 = (aft1 - pre1) / time_count
    delta2 = (aft2 - pre2) / time_count

    curr_time = datetime.now()
    minute = curr_time.minute
    len1 = (minute // delta_time + 1) * delta1 + pre1
    len2 = (minute // delta_time + 1) * delta2 + pre2
    return round(len1,1),round(len2,1)


