import json
import psycopg2

def getKeysOfDictFromCurrentRow(s,tDict, num):
    count = 0
    for k, v in tDict.items():
        if isinstance(v,dict):
            s = getKeysOfDictFromCurrentRow(s,v,num + 1) + ','
            continue
        s = s + str(k).replace("-","_") + ","
       # count = count + 1
       # if count  == maxi:
       #     break
    s = s[:-1]
    return s
    
def getValuesOfDictFromCurrentRow(s,tDict, num):
    count = 0
    for k, v in tDict.items():
        if isinstance(v,dict):
            s = getValuesOfDictFromCurrentRow(s,v,num + 1) + ','
            continue
        if isinstance(v,bool) == True or isinstance(v,int) == True:
            s = s + str(v) + ","
        else:
            s = s + "'" + str(v) + "', "
       # count = count + 1
       # if count  == maxi:
       #     break
    s = s[:-1]
    return s


def cleanStr4SQL(s):
    return s.replace("'","`").replace("\n"," ")

def int2BoolStr (value):
    if value == 0:
        return 'False'
    else:
        return 'True'

def insert2BusinessTable():
    #reading the JSON file
    logList = []

    with open('.//yelp_dataset//yelp_business.JSON','r') as f:    #TODO: update path for the input file
        #outfile =  open('.//yelp_dataset//yelp_business.SQL', 'w')  #uncomment this line if you are writing the INSERT statements to an output file.
        line = f.readline()
        count_line = 0

        #connect to yelpdb database on postgres server using psycopg2
        #TODO: update the database name, username, and password
        try:
            conn = psycopg2.connect("dbname='Milestone2DB' user='postgres' host='localhost' password='minh1234'")
        except:
            print('Unable to connect to the database!')
        cur = conn.cursor()
        
        while line:
            data = json.loads(line)
            sql_str = ""
            sql_str = "INSERT INTO yelp_business_entity (business_id, name, neighborhood, address, city, state, postal_code, latitude, longitude, stars, review_count, numCheckins, is_open, reviewrating, categories) " \
                        "VALUES ('" + cleanStr4SQL(data['business_id']) + "','" + cleanStr4SQL(data['name']) + "','" + cleanStr4SQL(data["neighborhood"]) + \
                        "','" + cleanStr4SQL(data['address']) + "','" + cleanStr4SQL(data['city']) + "','" + cleanStr4SQL(data['state']) + "','" + \
                        cleanStr4SQL(data['postal_code']) + "'," + str(data['latitude']) + "," + str(data['longitude']) + \
                        "," + str(data['stars']) + "," + str(data['review_count']) + ",0" + "," +  int2BoolStr(data["is_open"]) + ",0.0" + "," + \
                        "'{" + (','.join(data["categories"])).replace("'","''") + "}');"        
            try:
                cur.execute(sql_str)
            except:
                print("Insert to businessTABLE failed!")
                return   
            conn.commit()
            line = f.readline()
            count_line +=1
        cur.close()
        conn.close()

    print(count_line)
    #outfile.close()  #uncomment this line if you are writing the INSERT statements to an output file.
    f.close()

def insert2CategoriesTable():
    with open('.//yelp_dataset//yelp_business.JSON','r') as f:
        line = f.readline()
        count_line = 0
        try:
            conn = psycopg2.connect("dbname='Milestone2DB' user='postgres' host='localhost' password='minh1234'")
        except:
            print('Unable to connect to the database!')
        cur = conn.cursor()
        while line:
            data = json.loads(line)
            for item in data["categories"]:
                sql_str = "INSERT INTO categories_entity (business_id,categories"\
                      + ")" + "VALUES" + "('" + cleanStr4SQL(data['business_id']) + "','" + cleanStr4SQL(item) + "')"                
                try:
                    cur.execute(sql_str)
                except:
                    print("Insert to userTABLE failed!")
                    return
            conn.commit()
            line = f.readline()
            count_line +=1
        cur.close()
        conn.close()
    print(count_line)
    f.close()

def insert2AttributeTable():
    #reading the JSON file
    logList = []

    with open('.//yelp_dataset//yelp_business.JSON','r') as f:    
        line = f.readline()
        count_line = 0
        
        try:
            conn = psycopg2.connect("dbname='Milestone2DB' user='postgres' host='localhost' password='minh1234'")
        except:
            print('Unable to connect to the database!')
        cur = conn.cursor()
        
        while line:
            data = json.loads(line)
            if len(data['attributes']) !=  0:
                sql_str = ""
                index = 0
                for item in (getKeysOfDictFromCurrentRow("",data['attributes'], 0)).split(","):
                    sql_str = "INSERT INTO attributes_entity (business_id, name, value) VALUES ('" + cleanStr4SQL(data['business_id']) + "','" + item + "','" + ((getValuesOfDictFromCurrentRow("",data['attributes'], 0)).split(","))[index].replace("'","") +"')"
                    index = index + 1
                    try:

                        cur.execute(sql_str)
                    except:         
                        print("Insert to attributeTABLE failed!")
                        return
                    conn.commit()                   
            line = f.readline()
            count_line +=1
        cur.close()
        conn.close()

    print(count_line)
    #outfile.close()  #uncomment this line if you are writing the INSERT statements to an output file.
    f.close()

def insert2HourTable():
    #reading the JSON file
    logList = []

    with open('.//yelp_dataset//yelp_business.JSON','r') as f:    
        line = f.readline()
        count_line = 0
        
        try:
            conn = psycopg2.connect("dbname='Milestone2DB' user='postgres' host='localhost' password='minh1234'")
        except:
            print('Unable to connect to the database!')
        cur = conn.cursor()
        
        while line:
            data = json.loads(line)
            if len(data['hours']) !=  0:
                sql_str = ""
                index = 0
                for item in (getKeysOfDictFromCurrentRow("",data['hours'], 0)).split(","):
                    sql_str = "INSERT INTO hour_entity (business_id, the_date, the_time) VALUES ('" + cleanStr4SQL(data['business_id']) + "','" + item + "','" + ((getValuesOfDictFromCurrentRow("",data['hours'], 0)).split(","))[index].replace("'","") +"')"
                    index = index + 1                
                    try:

                        cur.execute(sql_str)
                    except:         
                        print("Insert to hourTABLE failed!")
                        return
                    conn.commit()                   
            line = f.readline()
            count_line +=1
        cur.close()
        conn.close()

    print(count_line)
    #outfile.close()  #uncomment this line if you are writing the INSERT statements to an output file.
    f.close()

def insert2UserTable():
    with open('.//yelp_dataset//yelp_user.JSON','r') as f:
        line = f.readline()
        count_line = 0
        try:
            conn = psycopg2.connect("dbname='Milestone2DB' user='postgres' host='localhost' password='minh1234'")
        except:
            print('Unable to connect to the database!')
        cur = conn.cursor()
        while line:
            data = json.loads(line)
            sql_str = "INSERT INTO yelp_user_entity (user_id,name,elite,average_stars,cool,fans,funny,useful,review_count,yelping_since"\
                      + ")" + "VALUES" + "('" + cleanStr4SQL(data['user_id']) + "','" + cleanStr4SQL(data['name']) \
                      + "','{" + (','.join(str(x) for x in data["elite"])) + "}" + "'," + str(data['average_stars']) \
                      + "," + str(data['cool']) + "," + str(data['fans']) + "," + str(data['funny']) \
                      + "," + str(data['useful']) + "," + str(data['review_count']) + "," + "DATE '" + str(data['yelping_since']) + "'" \
                      + ")"
            try:
                cur.execute(sql_str)
            except:
                print("Insert to userTABLE failed!")
                return    
            conn.commit()
            line = f.readline()
            count_line +=1
        cur.close()
        conn.close()
    print(count_line)
    f.close()

def insert2ReviewTable():
    with open('.//yelp_dataset//yelp_review.JSON','r') as f:
        line = f.readline()
        count_line = 0
        try:
            conn = psycopg2.connect("dbname='Milestone2DB' user='postgres' host='localhost' password='minh1234'")
        except:
            print('Unable to connect to the database!')
        cur = conn.cursor()
        while line:
            data = json.loads(line)
            
            sql_str = "INSERT INTO review_entity (review_id,business_id,user_id,text,date,stars,useful,funny,cool"\
                      + ")" + "VALUES" + "('" + cleanStr4SQL(data['review_id']) + "','" + cleanStr4SQL(data['business_id']) + "','" + cleanStr4SQL(data['user_id'])+ "','" + cleanStr4SQL(data['text']) \
                      + "'," + "DATE '" + str(data['date']) + "'," + str(data['stars']) + "," + str(data['useful']) \
                      + "," + str(data['funny']) + "," + str(data['cool']) + ")"
            try:
                cur.execute(sql_str)
            except:
                print("Insert to reviewTABLE failed!")
                return   
            conn.commit()
            line = f.readline()
            count_line +=1
        cur.close()
        conn.close()
    print(count_line)
    f.close()

def insert2CheckinTable():
    morning_times = ['6:00', '7:00', '8:00', '9:00', '10:00', '11:00']
    afternoon_times = ['12:00', '13:00', '14:00', '15:00', '16:00']
    evening_times = ['17:00', '18:00', '19:00', '20:00', '21:00', '22:00']
    night_times = ['23:00', '0:00', '1:00', '2:00', '3:00', '4:00', '5:00']
    
    with open('.//yelp_dataset//yelp_checkin.JSON','r') as f:
        line = f.readline()
        count_line = 0
        try:
            conn = psycopg2.connect("dbname='Milestone2DB' user='postgres' host='localhost' password='minh1234'")
        except:
            print('Unable to connect to the database!')
        cur = conn.cursor()
        while line:
            data = json.loads(line)
            for day_of_week, check_in_times in data['time'].items():
                morning_count = 0
                afternoon_count = 0
                evening_count = 0
                night_count = 0
                for time, check_in_num in check_in_times.items():
                    if time in morning_times:
                        morning_count += check_in_num
                    elif time in afternoon_times:
                        afternoon_count += check_in_num
                    elif time in evening_times:
                        evening_count += check_in_num
                    elif time in night_times:
                        night_count += check_in_num
                sql_str = "INSERT INTO checkin_entity (business_id,date,morning,afternoon,evening,night"\
                          + ")" + "VALUES" + "('" + cleanStr4SQL(data['business_id']) + "','" + day_of_week + "'," + str(morning_count) \
                          + "," + str(afternoon_count) + "," + str(evening_count) + "," + str(night_count)+ ")"  
                try:
                    cur.execute(sql_str)
                except:
                    print("Insert to checkinTABLE failed!")
                    return    
                conn.commit()
            line = f.readline()
            count_line +=1
        cur.close()
        conn.close()
    print(count_line)
    f.close()

def insert2isFriendTable():
    with open('.//yelp_dataset//yelp_user.JSON','r') as f:
        line = f.readline()
        count_line = 0
        try:
            conn = psycopg2.connect("dbname='Milestone2DB' user='postgres' host='localhost' password='minh1234'")
        except:
            print('Unable to connect to the database!')
        cur = conn.cursor()
        while line:
            data = json.loads(line)
            for item in data["friends"]:
                sql_str = "INSERT INTO isFriend_Relationship (user_id_one,user_id_two) VALUES ('" + cleanStr4SQL(data['user_id']) \
                          + "','" + item  + "')"
                try:
                    cur.execute(sql_str)
                except:
                    print("Insert to isFriend_RelationshipTABLE failed!")
                    return    
                conn.commit()
            line = f.readline()
            count_line +=1
        cur.close()
        conn.close()
    print(count_line)
    f.close()


insert2BusinessTable()
insert2AttributeTable()
insert2HourTable()
insert2CategoriesTable()
insert2UserTable()
insert2isFriendTable()
insert2ReviewTable()
insert2CheckinTable()


