DROP TABLE IF EXISTS coi_database.role_and_guild_connection;

CREATE TABLE coi_database.role_and_guild_connection (
	id INT PRIMARY KEY,	
	guild_id VARCHAR(20),
	role_name VARCHAR(100),
	income INT	
);