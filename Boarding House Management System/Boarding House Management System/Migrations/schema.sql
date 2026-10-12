CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE TABLE `dormitories` (
        `dormitory_id` int NOT NULL AUTO_INCREMENT,
        `name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `address` varchar(255) CHARACTER SET utf8mb4 NULL,
        `contact_number` varchar(20) CHARACTER SET utf8mb4 NULL,
        `description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_dormitories` PRIMARY KEY (`dormitory_id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE TABLE `users` (
        `user_id` int NOT NULL AUTO_INCREMENT,
        `username` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `password_hash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `full_name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `email` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `phone_number` varchar(20) CHARACTER SET utf8mb4 NULL,
        `role` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_users` PRIMARY KEY (`user_id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE TABLE `rooms` (
        `room_id` int NOT NULL AUTO_INCREMENT,
        `dormitory_id` int NULL,
        `room_number` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `room_type` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `capacity` int NOT NULL,
        `current_occupants` int NOT NULL,
        `monthly_rate` decimal(18,2) NOT NULL,
        `status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_rooms` PRIMARY KEY (`room_id`),
        CONSTRAINT `FK_rooms_dormitories_dormitory_id` FOREIGN KEY (`dormitory_id`) REFERENCES `dormitories` (`dormitory_id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE TABLE `leases` (
        `lease_id` int NOT NULL AUTO_INCREMENT,
        `user_id` int NOT NULL,
        `room_id` int NOT NULL,
        `start_date` datetime(6) NOT NULL,
        `end_date` datetime(6) NOT NULL,
        `status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_leases` PRIMARY KEY (`lease_id`),
        CONSTRAINT `FK_leases_rooms_room_id` FOREIGN KEY (`room_id`) REFERENCES `rooms` (`room_id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_leases_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE TABLE `maintenance_requests` (
        `request_id` int NOT NULL AUTO_INCREMENT,
        `room_id` int NOT NULL,
        `user_id` int NOT NULL,
        `issue_description` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
        `date_reported` datetime(6) NOT NULL,
        `status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `resolved_at` datetime(6) NULL,
        CONSTRAINT `PK_maintenance_requests` PRIMARY KEY (`request_id`),
        CONSTRAINT `FK_maintenance_requests_rooms_room_id` FOREIGN KEY (`room_id`) REFERENCES `rooms` (`room_id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_maintenance_requests_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE TABLE `reservations` (
        `reservation_id` int NOT NULL AUTO_INCREMENT,
        `user_id` int NOT NULL,
        `room_id` int NOT NULL,
        `reservation_date` datetime(6) NOT NULL,
        `status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_reservations` PRIMARY KEY (`reservation_id`),
        CONSTRAINT `FK_reservations_rooms_room_id` FOREIGN KEY (`room_id`) REFERENCES `rooms` (`room_id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_reservations_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE TABLE `payments` (
        `payment_id` int NOT NULL AUTO_INCREMENT,
        `lease_id` int NOT NULL,
        `amount_paid` decimal(18,2) NOT NULL,
        `payment_date` datetime(6) NOT NULL,
        `payment_method` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `reference_number` varchar(100) CHARACTER SET utf8mb4 NULL,
        `status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_payments` PRIMARY KEY (`payment_id`),
        CONSTRAINT `FK_payments_leases_lease_id` FOREIGN KEY (`lease_id`) REFERENCES `leases` (`lease_id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE INDEX `IX_leases_room_id` ON `leases` (`room_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE INDEX `IX_leases_user_id` ON `leases` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE INDEX `IX_maintenance_requests_room_id` ON `maintenance_requests` (`room_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE INDEX `IX_maintenance_requests_user_id` ON `maintenance_requests` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE INDEX `IX_payments_lease_id` ON `payments` (`lease_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE INDEX `IX_reservations_room_id` ON `reservations` (`room_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE INDEX `IX_reservations_user_id` ON `reservations` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    CREATE INDEX `IX_rooms_dormitory_id` ON `rooms` (`dormitory_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261011233420_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261011233420_InitialCreate', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

