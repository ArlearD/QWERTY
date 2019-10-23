CREATE DATABASE dotnet;
\connect dotnet

CREATE TABLE product (
	price numeric,
	id serial PRIMARY KEY,
);