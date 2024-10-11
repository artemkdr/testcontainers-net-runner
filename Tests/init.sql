-- create some tables on start
-- OR you can use context.Database.EnsureCreate() if you use EntityFramework and the models
CREATE TABLE users (
	username text PRIMARY KEY,
	"password" text NULL,
	vars jsonb NULL,
	"status" text,
	createdate timestamp with time zone,
	modifydate timestamp with time zone
);