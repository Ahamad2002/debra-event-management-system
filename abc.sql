-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Nov 07, 2024 at 09:31 AM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `abc`
--

-- --------------------------------------------------------

--
-- Table structure for table `commission`
--

CREATE TABLE `commission` (
  `CommissionID` int(11) NOT NULL,
  `EventID` int(11) DEFAULT NULL,
  `CommissionRate` decimal(5,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `commission`
--

INSERT INTO `commission` (`CommissionID`, `EventID`, `CommissionRate`) VALUES
(5, 19, 10.00),
(6, 20, 5.00),
(7, 21, 15.00),
(8, 16, 20.00),
(9, 17, 15.00),
(10, 18, 10.00);

-- --------------------------------------------------------

--
-- Table structure for table `event`
--

CREATE TABLE `event` (
  `EventID` int(11) NOT NULL,
  `EventName` varchar(100) NOT NULL,
  `EventDescription` text DEFAULT NULL,
  `EventDate` date NOT NULL,
  `Venue` varchar(255) NOT NULL,
  `UserID` int(11) DEFAULT NULL,
  `ImagePath` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `event`
--

INSERT INTO `event` (`EventID`, `EventName`, `EventDescription`, `EventDate`, `Venue`, `UserID`, `ImagePath`) VALUES
(16, 'Music Fest 2024', 'A grand musical event featuring performances from top artists around the world.', '2024-08-14', 'Central Park, New York', 3, 'images\\662c78e8-5d9f-4453-8ed4-26ea942dfc09_rq.jpeg'),
(17, 'Tech Conference 2024', 'Annual conference for tech enthusiasts to discuss the latest trends and innovations in technology.', '2024-07-18', 'Silicon Valley Convention Center, California', 3, 'images\\12bc992a-b6dd-4885-adff-ffb9f6c20cb0_dd.jpeg'),
(18, 'Food Expo 2024', 'A culinary event showcasing dishes from renowned chefs and food vendors.', '2024-07-27', ' Expo Center, Chicago', 3, 'images\\319d70d8-6201-4648-8512-22b104976e16_34.jpeg'),
(19, 'Art Exhibition 2024', 'An exhibition featuring contemporary art pieces from various artists.', '2024-07-31', 'Art Gallery, Los Angeles', 4, 'images\\5df1a3c5-678e-4025-87e6-1c47eb1bb752_ss.jpeg'),
(20, 'Marathon 2024', 'A city-wide marathon event encouraging fitness and community participation.', '2024-07-26', 'Downtown, Boston', 4, 'images\\7d9d0cfe-6b9c-44a7-ad67-1ae2531e0b95_jj.jpeg'),
(21, 'Film Festival 2024', 'An international film festival showcasing independent and blockbuster films.', '2024-09-24', 'Film Center, Toronto', 4, 'images\\1726fb34-9650-4404-920e-26040562c985_er.jpeg');

-- --------------------------------------------------------

--
-- Table structure for table `ticketdetails`
--

CREATE TABLE `ticketdetails` (
  `TicketDetailsID` int(11) NOT NULL,
  `EventID` int(11) DEFAULT NULL,
  `TicketType` varchar(50) NOT NULL,
  `TicketPrice` decimal(10,2) NOT NULL,
  `TicketQuantity` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `ticketdetails`
--

INSERT INTO `ticketdetails` (`TicketDetailsID`, `EventID`, `TicketType`, `TicketPrice`, `TicketQuantity`) VALUES
(14, 16, 'First Class', 1200.00, 490),
(15, 16, 'Second Class', 600.00, 995),
(16, 17, 'First Class', 2000.00, 600),
(17, 17, 'Second Class', 1000.00, 1394),
(18, 18, 'First Class', 600.00, 993),
(19, 18, 'Second Class', 300.00, 990),
(20, 19, 'First Class', 600.00, 490),
(21, 19, 'Second Class', 300.00, 995),
(22, 20, 'First Class', 400.00, 450),
(23, 20, 'Second Class', 200.00, 990),
(24, 21, 'First Class', 1500.00, 990),
(25, 21, 'Second Class', 800.00, 1945);

-- --------------------------------------------------------

--
-- Table structure for table `ticketsale`
--

CREATE TABLE `ticketsale` (
  `TicketSaleID` int(11) NOT NULL,
  `UserID` int(11) DEFAULT NULL,
  `EventID` int(11) DEFAULT NULL,
  `TicketDetailsID` int(11) DEFAULT NULL,
  `SaleDate` datetime NOT NULL,
  `SaleStatus` enum('Pending','Completed','Cancelled') NOT NULL,
  `SaleAmount` decimal(10,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `ticketsale`
--

INSERT INTO `ticketsale` (`TicketSaleID`, `UserID`, `EventID`, `TicketDetailsID`, `SaleDate`, `SaleStatus`, `SaleAmount`) VALUES
(6, 2, 16, 14, '2024-07-02 02:30:37', 'Completed', 12000.00),
(7, 2, 16, 15, '2024-07-02 02:30:42', 'Completed', 3000.00),
(8, 2, 17, 17, '2024-07-02 02:30:59', 'Completed', 6000.00),
(9, 2, 18, 18, '2024-07-02 02:31:16', 'Completed', 4200.00),
(10, 2, 18, 19, '2024-07-02 02:31:20', 'Completed', 3000.00),
(11, 6, 19, 20, '2024-07-02 02:32:44', 'Completed', 6000.00),
(12, 6, 19, 21, '2024-07-02 02:32:48', 'Completed', 1500.00),
(13, 6, 20, 22, '2024-07-02 02:33:04', 'Completed', 20000.00),
(14, 6, 20, 23, '2024-07-02 02:33:09', 'Completed', 2000.00),
(15, 6, 21, 24, '2024-07-02 02:33:24', 'Completed', 15000.00),
(16, 6, 21, 25, '2024-07-02 02:33:35', 'Completed', 44000.00);

-- --------------------------------------------------------

--
-- Table structure for table `user1`
--

CREATE TABLE `user1` (
  `UserID` int(11) NOT NULL,
  `Username` varchar(50) NOT NULL,
  `Password` varchar(100) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `MobileNumber` varchar(255) DEFAULT NULL,
  `UserType` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `user1`
--

INSERT INTO `user1` (`UserID`, `Username`, `Password`, `Email`, `MobileNumber`, `UserType`) VALUES
(0, 'Admin', 'admin1', 'admin@gmail.com', '0754859634', 'Admin'),
(2, 'user', 'user1', 'user@gmail.com', '0756252345', 'User'),
(3, 'user1', 'user11', 'user1@gmail.com', '0724561234', 'Partner'),
(4, 'user2', 'user2', 'user2@gmail.com', '0734561234', 'Partner'),
(6, 'user3', 'user3', 'user3@gmail.com', '0754859634', 'User');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `commission`
--
ALTER TABLE `commission`
  ADD PRIMARY KEY (`CommissionID`),
  ADD KEY `EventID` (`EventID`);

--
-- Indexes for table `event`
--
ALTER TABLE `event`
  ADD PRIMARY KEY (`EventID`),
  ADD KEY `UserID` (`UserID`);

--
-- Indexes for table `ticketdetails`
--
ALTER TABLE `ticketdetails`
  ADD PRIMARY KEY (`TicketDetailsID`),
  ADD KEY `EventID` (`EventID`);

--
-- Indexes for table `ticketsale`
--
ALTER TABLE `ticketsale`
  ADD PRIMARY KEY (`TicketSaleID`),
  ADD KEY `UserID` (`UserID`),
  ADD KEY `EventID` (`EventID`),
  ADD KEY `TicketDetailsID` (`TicketDetailsID`);

--
-- Indexes for table `user1`
--
ALTER TABLE `user1`
  ADD PRIMARY KEY (`UserID`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `commission`
--
ALTER TABLE `commission`
  MODIFY `CommissionID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `event`
--
ALTER TABLE `event`
  MODIFY `EventID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=22;

--
-- AUTO_INCREMENT for table `ticketdetails`
--
ALTER TABLE `ticketdetails`
  MODIFY `TicketDetailsID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=26;

--
-- AUTO_INCREMENT for table `ticketsale`
--
ALTER TABLE `ticketsale`
  MODIFY `TicketSaleID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;

--
-- AUTO_INCREMENT for table `user1`
--
ALTER TABLE `user1`
  MODIFY `UserID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `commission`
--
ALTER TABLE `commission`
  ADD CONSTRAINT `commission_ibfk_1` FOREIGN KEY (`EventID`) REFERENCES `event` (`EventID`);

--
-- Constraints for table `event`
--
ALTER TABLE `event`
  ADD CONSTRAINT `event_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `user1` (`UserID`);

--
-- Constraints for table `ticketdetails`
--
ALTER TABLE `ticketdetails`
  ADD CONSTRAINT `ticketdetails_ibfk_1` FOREIGN KEY (`EventID`) REFERENCES `event` (`EventID`);

--
-- Constraints for table `ticketsale`
--
ALTER TABLE `ticketsale`
  ADD CONSTRAINT `ticketsale_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `user1` (`UserID`),
  ADD CONSTRAINT `ticketsale_ibfk_2` FOREIGN KEY (`EventID`) REFERENCES `event` (`EventID`),
  ADD CONSTRAINT `ticketsale_ibfk_3` FOREIGN KEY (`TicketDetailsID`) REFERENCES `ticketdetails` (`TicketDetailsID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
