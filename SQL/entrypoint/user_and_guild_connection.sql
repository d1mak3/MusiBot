DROP TABLE IF EXISTS coi_database.user_and_guild_connection;

CREATE TABLE coi_database.user_and_guild_connection (
	id INT PRIMARY KEY,
	user_id VARCHAR(20),
	guild_id VARCHAR(20),
	earn_time DATETIME,
	role_income_time DATETIME
);