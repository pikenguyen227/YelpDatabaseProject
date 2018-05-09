/* UPDATE STATEMENTS */
 -- UPDATE THE NUMCHECKINS --
 UPDATE 
    yelp_business_entity
SET
    numcheckins = i.total
FROM (
    SELECT business_id, SUM(morning+afternoon+evening+night) AS total FROM checkin_entity GROUP BY business_id) i
WHERE
    i.business_id = yelp_business_entity.business_id;

-- UPDATE THE REVIEW_COUNT --
UPDATE 
    yelp_business_entity
SET
    review_count = i.total
FROM (
    SELECT business_id, COUNT(review_id) AS total FROM review_entity GROUP BY business_id) i
WHERE
    i.business_id = yelp_business_entity.business_id;

-- UPDATE THE REVIEWRATING --
UPDATE 
    yelp_business_entity
SET
    reviewrating = i.total
FROM (
    SELECT business_id, AVG(stars) AS total FROM review_entity GROUP BY business_id) i
WHERE
    i.business_id = yelp_business_entity.business_id;


