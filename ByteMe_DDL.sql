/*removing the old tables*/
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
/*************************/

CREATE TABLE yelp_business_entity
(
	business_id 	VARCHAR(22) PRIMARY KEY, 
	name 			VARCHAR(100),
    neighborhood  	VARCHAR(100),
    address			VARCHAR(100),
    city			VARCHAR(30),
    state			VARCHAR(15),
    postal_code		VARCHAR(5),
    latitude		REAL,
	longitude		REAL,
    stars			INT CHECK(stars > 0 AND stars <= 5),
    review_count    INT,
	is_open 		BOOL,
	reviewrating	FLOAT	CHECK(stars > 0 AND stars <= 5),
	numCheckins   	INT,
    categories		VARCHAR(50)[]
);

CREATE TABLE Yelp_User_Entity
(
	user_id			VARCHAR(22) PRIMARY KEY,
    name			VARCHAR(50),
	average_stars	INT	CHECK(average_stars > 0 AND average_stars <= 5),
	cool			INT,
	elite			INT[],
	fans			INT,
	funny			INT,
	review_count	INT,
	useful			INT,
	yelping_since	DATE
    --isFriend_to 	VARCHAR(22)[] --null REFERENCES Yelp_User_Entity(user_id)
);

CREATE TABLE Review_Entity
(
	review_id		VARCHAR(22)	PRIMARY KEY,
    business_id		VARCHAR(22),
    user_id			VARCHAR(22),
	stars			INT	CHECK(stars > 0 AND stars<=5),			
	date			DATE,
	text			TEXT,
	useful			INT,	
	funny			INT,
	cool			INT,
    FOREIGN KEY (business_id)	REFERENCES 	Yelp_Business_Entity(business_id),
    FOREIGN KEY (user_id) 		REFERENCES  Yelp_User_Entity(user_id)
);

CREATE TABLE Checkin_Entity
(
	business_id		VARCHAR(22),
	date			VARCHAR(10),
	morning			INT,	
	afternoon		INT,
	evening			INT,
    night			INT,
	PRIMARY KEY (business_id, date),
    FOREIGN KEY (business_id) REFERENCES Yelp_Business_Entity(business_id)
);

CREATE TABLE Attributes_Entity
(
	business_id		VARCHAR(22),
	name 			VARCHAR(50),
    value			VARCHAR(50),
    PRIMARY KEY (business_id,name),
    FOREIGN KEY (business_id) REFERENCES Yelp_Business_Entity(business_id) ON DELETE CASCADE
);

CREATE TABLE Hour_Entity
(
    business_id		VARCHAR(22),
	the_date 		VARCHAR(10),
    the_time 		VARCHAR(12),
    PRIMARY KEY (business_id,the_date),
    FOREIGN KEY (business_id) REFERENCES Yelp_Business_Entity(business_id) ON DELETE CASCADE
);

CREATE TABLE isFriend_Relationship
(
	user_id_one		VARCHAR(22),
    user_id_two		VARCHAR(22),
   	FOREIGN KEY (user_id_one) REFERENCES  Yelp_User_Entity(user_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id_two) REFERENCES  Yelp_User_Entity(user_id) ON DELETE CASCADE
);

CREATE TABLE newCustomerCheckin
(
	business_id 	VARCHAR(22),
    date			VARCHAR(10),
    time			VARCHAR(10),
    FOREIGN KEY (business_id) REFERENCES Yelp_Business_Entity(business_id) ON DELETE CASCADE
);

CREATE TABLE categories_entity
(
	business_id 	VARCHAR(22),
    categories		VARCHAR(50),
    PRIMARY KEY (business_id,categories),
    FOREIGN KEY (business_id) REFERENCES Yelp_Business_Entity(business_id) ON DELETE CASCADE
)