--DROP TRIGGER updateNum ON newcustomercheckin;
--DROP TRIGGER updatereview ON review_entity;
--DROP TRIGGER rejectInsert ON review_entity;
--DROP TRIGGER deleteNum ON newcustomercheckin;
--DROP TRIGGER updateCheckinEntity ON checkin_entity;

/* TRIGGERS */
-- TRIGGERS THE NUMCHECKINS --
CREATE OR REPLACE FUNCTION updateNumCheckins() RETURNS trigger
	AS '
    BEGIN
    	IF NEW.time = ''morning'' 
        	THEN 
            	UPDATE 
                	checkin_entity 
               	SET
                	morning = morning + 1
               	FROM
                	yelp_business_entity
                WHERE 
                	NEW.business_id = checkin_entity.business_id 
               	AND NEW.date= checkin_entity.date 
                AND yelp_business_entity.business_id = checkin_entity.business_id
                AND yelp_business_entity.is_open = true;
		ELSIF NEW.time = ''afternoon'' 
        	THEN 
            	UPDATE 
                	checkin_entity 
               	SET
                	afternoon = afternoon + 1
               	FROM
                	yelp_business_entity
                WHERE 
                	NEW.business_id = checkin_entity.business_id 
               	AND NEW.date= checkin_entity.date 
                AND yelp_business_entity.business_id = checkin_entity.business_id
                AND yelp_business_entity.is_open = true;
        ELSIF NEW.time = ''evening'' 
        	THEN 
            	UPDATE 
                	checkin_entity 
               	SET
                	evening = evening + 1
               	FROM
                	yelp_business_entity
                WHERE 
                	NEW.business_id = checkin_entity.business_id 
               	AND NEW.date= checkin_entity.date 
                AND yelp_business_entity.business_id = checkin_entity.business_id
                AND yelp_business_entity.is_open = true;
         ELSIF NEW.time = ''night'' 
        	THEN 
            	UPDATE 
                	checkin_entity 
               	SET
                	night = night + 1
               	FROM
                	yelp_business_entity
                WHERE 
                	NEW.business_id = checkin_entity.business_id 
               	AND NEW.date= checkin_entity.date 
                AND yelp_business_entity.business_id = checkin_entity.business_id
                AND yelp_business_entity.is_open = true;
        END IF;
        RETURN NEW;
   END
    ' LANGUAGE plpgsql
;

CREATE TRIGGER updateNum
	AFTER INSERT ON newcustomercheckin
    FOR EACH ROW 
    EXECUTE PROCEDURE updateNumCheckins();
    
-- TRIGGERS THE NUMCHECKINS --
CREATE OR REPLACE FUNCTION deleteNumCheckins() RETURNS trigger
	AS '
    BEGIN
    	IF OLD.time = ''morning'' 
        	THEN 
            	UPDATE 
                	checkin_entity 
               	SET
                	morning = morning - 1
               	FROM
                	yelp_business_entity
                WHERE 
                	OLD.business_id = checkin_entity.business_id 
               	AND OLD.date= checkin_entity.date 
                AND yelp_business_entity.business_id = checkin_entity.business_id;
             
		ELSIF OLD.time = ''afternoon'' 
        	THEN 
            	UPDATE 
                	checkin_entity 
               	SET
                	afternoon = afternoon - 1
               	FROM
                	yelp_business_entity
                WHERE 
                	OLD.business_id = checkin_entity.business_id 
               	AND OLD.date= checkin_entity.date 
                AND yelp_business_entity.business_id = checkin_entity.business_id;
                
        ELSIF OLD.time = ''evening'' 
        	THEN 
            	UPDATE 
                	checkin_entity 
               	SET
                	evening = evening - 1
               	FROM
                	yelp_business_entity
                WHERE 
                	OLD.business_id = checkin_entity.business_id 
               	AND OLD.date= checkin_entity.date 
                AND yelp_business_entity.business_id = checkin_entity.business_id;
                
         ELSIF OLD.time = ''night'' 
        	THEN 
            	UPDATE 
                	checkin_entity 
               	SET
                	night = night - 1
               	FROM
                	yelp_business_entity
                WHERE 
                	OLD.business_id = checkin_entity.business_id 
               	AND OLD.date= checkin_entity.date 
                AND yelp_business_entity.business_id = checkin_entity.business_id;
                
        END IF;
        RETURN NEW;
   END
    ' LANGUAGE plpgsql
;

CREATE TRIGGER deleteNum
	AFTER DELETE ON newcustomercheckin
    FOR EACH ROW 
    EXECUTE PROCEDURE deleteNumCheckins();    

-- TRIGGERS THE CHECKIN_ENTITY --
CREATE OR REPLACE FUNCTION updateCheckins() RETURNS trigger
	AS '
    BEGIN
        UPDATE 
            yelp_business_entity
        SET
            numcheckins = i.total
        FROM (
            SELECT business_id, SUM(morning+afternoon+evening+night) AS total FROM checkin_entity GROUP BY business_id) i
        WHERE
            i.business_id = yelp_business_entity.business_id AND yelp_business_entity.is_open = true;
        RETURN NEW;
   END
    ' LANGUAGE plpgsql
;

CREATE TRIGGER updateCheckinEntity
	AFTER UPDATE OF morning,afternoon,evening,night ON checkin_entity
    FOR EACH ROW 
    EXECUTE PROCEDURE updateCheckins();  

-- TRIGGERS THE REVIEW_COUNT AND THE REVIEWRATIN --
CREATE OR REPLACE FUNCTION updateReviewCR() RETURNS trigger
	AS '
    BEGIN
        UPDATE 
    		yelp_business_entity
        SET
            review_count = i.total
        FROM (
            SELECT business_id, COUNT(review_id) AS total FROM review_entity GROUP BY business_id) i
        WHERE
            i.business_id = yelp_business_entity.business_id;
            
        UPDATE 
            yelp_business_entity
        SET
            reviewrating = i.total
        FROM (
            SELECT business_id, AVG(stars) AS total FROM review_entity GROUP BY business_id) i
        WHERE
            i.business_id = yelp_business_entity.business_id;  
       
        RETURN NEW;
    END
    ' LANGUAGE plpgsql
;

CREATE TRIGGER updateReview
	AFTER INSERT ON review_entity
    FOR EACH ROW
    EXECUTE PROCEDURE updateReviewCR();

-- EXTRA CREDIT --
CREATE OR REPLACE FUNCTION rejectReviewCR() RETURNS trigger
	AS '
    BEGIN
        IF (SELECT yelp_business_entity.is_open FROM yelp_business_entity WHERE yelp_business_entity.business_id = NEW.business_id) = false
        THEN RETURN NULL;
        END IF;
        RETURN NEW;
    END
    ' LANGUAGE plpgsql
;

CREATE TRIGGER rejectInsert
	BEFORE INSERT ON review_entity
    FOR EACH ROW
    EXECUTE PROCEDURE rejectReviewCR();

--TEST numcheckins
--INSERT INTO newcustomercheckin (business_id,date,time) VALUES ('--ab39IjZR_xUf81WyTyHg', 'Friday', 'morning');
--INSERT INTO newcustomercheckin (business_id,date,time) VALUES ('--ab39IjZR_xUf81WyTyHg', 'Monday', 'afternoon');
--INSERT INTO newcustomercheckin (business_id,date,time) VALUES ('--ab39IjZR_xUf81WyTyHg', 'Saturday', 'evening');
--INSERT INTO newcustomercheckin (business_id,date,time) VALUES ('--ab39IjZR_xUf81WyTyHg', 'Friday', 'night');
--DELETE FROM newcustomercheckin WHERE business_id = '--ab39IjZR_xUf81WyTyHg' AND DATE = 'Friday';
--DELETE FROM newcustomercheckin WHERE business_id = '--ab39IjZR_xUf81WyTyHg' AND DATE = 'Monday';
--UPDATE checkin_entity SET morning = 3 WHERE business_id = '--ab39IjZR_xUf81WyTyHg' AND date = 'Friday';
--TEST review_count and reviewrating  -050d_XIor1NpCuWkbIVaQ
--INSERT INTO review_entity (review_id,business_id,user_id,stars,date,text,useful,funny,cool) VALUES ('0000111111110101111111','--ab39IjZR_xUf81WyTyHg','T5MGS0NHBCWgofZ6Q6Btng',3,'2013-06-03','TESTING',0,0,0);
--SELECT * FROM review_entity WHERE business_id = '--ab39IjZR_xUf81WyTyHg'
-- TEST extra credit part
--INSERT INTO review_entity (review_id,business_id,user_id,stars,date,text,useful,funny,cool) VALUES ('0000011111111111100000','-050d_XIor1NpCuWkbIVaQ','T5MGS0NHBCWgofZ6Q6Btng',3,'2013-06-03','TESTING',0,0,0);
