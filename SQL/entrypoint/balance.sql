DROP TABLE IF EXISTS coi_database.balance;

CREATE TABLE coi_database.balance (
	id INT PRIMARY KEY,
	id_of_user_and_guild_connection INT,
	amount_of_coins_on_deposit INT,
	amount_of_coins_in_cash INT
);