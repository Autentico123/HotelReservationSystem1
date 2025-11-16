-- Create Payments table manually
-- Run this SQL script in your MySQL database if you can't stop the application

USE hotelreservationdb;

CREATE TABLE IF NOT EXISTS `Payments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ReservationId` int NOT NULL,
    `PaymentMethod` varchar(20) NOT NULL,
    `PaymentStatus` varchar(20) NOT NULL,
    `Amount` decimal(18,2) NOT NULL,
    `GCashNumber` varchar(15) NULL,
    `GCashReferenceNumber` varchar(50) NULL,
    `CreatedAt` datetime NOT NULL,
    `PaidAt` datetime NULL,
    `Notes` varchar(500) NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_Payments_ReservationId` (`ReservationId`),
    CONSTRAINT `FK_Payments_Reservations_ReservationId` 
        FOREIGN KEY (`ReservationId`) 
        REFERENCES `Reservations` (`Id`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
