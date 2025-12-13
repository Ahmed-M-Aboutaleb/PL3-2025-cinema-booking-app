CREATE DATABASE cinema_db;
USE cinema_db;

DROP TABLE IF EXISTS bookings;
DROP TABLE IF EXISTS movies;
DROP TABLE IF EXISTS halls;

CREATE TABLE halls (
    Id VARCHAR(50) PRIMARY KEY,
    Name VARCHAR(50),
    RowsInt INT,
    ColsInt INT
);

INSERT INTO halls VALUES ('h1', 'Standard Hall 1', 5, 5);
INSERT INTO halls VALUES ('h2', 'IMAX Experience', 8, 12);
INSERT INTO halls VALUES ('h3', 'VIP Lounge', 3, 4);

CREATE TABLE movies (
    Id VARCHAR(50) PRIMARY KEY,
    Title VARCHAR(100),
    ShowTime VARCHAR(20),
    Price DECIMAL(10,2),
    HallId VARCHAR(50),
    FOREIGN KEY (HallId) REFERENCES halls(Id)
);

CREATE TABLE bookings (
    MovieId VARCHAR(50),
    RowIdx INT,
    ColIdx INT,
    CustomerTicketId VARCHAR(100),
    BookingTime DATETIME,
    PRIMARY KEY (MovieId, RowIdx, ColIdx),
    FOREIGN KEY (MovieId) REFERENCES movies(Id)
);