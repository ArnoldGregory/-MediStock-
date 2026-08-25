-- ============================================================
--  MediStock — Fix Login SPs
--  Run this AFTER the initial 5 SQL files
--  Fixes: parameter names matching DBHandler, missing password column
-- ============================================================

USE medistock;

-- ── FIX validate_login (params must match DBHandler: @username, @profiletype) ──
DROP PROCEDURE IF EXISTS `validate_login`$$
CREATE PROCEDURE `validate_login`(
    IN username    VARCHAR(200),
    IN profiletype VARCHAR(50)
)
BEGIN
    SELECT id, pharmacy_id, role_id, first_name, middle_name, last_name,
           email, mobile, password, avatar, locked, change_password,
           failed_login_attempts, google_authenticate, sec_key
    FROM pharmacy_users
    WHERE email = username AND is_deleted = 0
    LIMIT 1;
END$$

-- ── FIX add_refresh_token (DBHandler sends @p_hashed_token) ──
DROP PROCEDURE IF EXISTS `add_refresh_token`$$
CREATE PROCEDURE `add_refresh_token`(
    IN p_user_id     BIGINT,
    IN p_hashed_token VARCHAR(500),
    IN p_expires_at  DATETIME
)
BEGIN
    INSERT INTO refresh_tokens (user_id, token, expires_at, created_on)
    VALUES (p_user_id, p_hashed_token, p_expires_at, NOW());
END$$

-- ── FIX riziki_save_otp (params must match DBHandler: @in_*) ──
DROP PROCEDURE IF EXISTS `riziki_save_otp`$$
CREATE PROCEDURE `riziki_save_otp`(
    IN  in_user_id    BIGINT,
    IN  in_user_type  VARCHAR(20),
    IN  in_email      VARCHAR(200),
    IN  in_mobile     VARCHAR(50),
    IN  in_otp_code   VARCHAR(10),
    IN  in_purpose    VARCHAR(50),
    IN  in_otp_ref    VARCHAR(100),
    OUT out_id         BIGINT
)
BEGIN
    SET out_id = 0;
    INSERT INTO otp_records
        (user_id, user_type, email, mobile, otp_code, purpose, otp_ref, verified, expires_at, created_on)
    VALUES
        (in_user_id, in_user_type, in_email, in_mobile, in_otp_code, in_purpose, in_otp_ref, 0,
         DATE_ADD(NOW(), INTERVAL 15 MINUTE), NOW());
    SET out_id = LAST_INSERT_ID();
END$$

-- ── FIX riziki_verify_otp (params must match DBHandler: @in_*) ──
DROP PROCEDURE IF EXISTS `riziki_verify_otp`$$
CREATE PROCEDURE `riziki_verify_otp`(
    IN in_email    VARCHAR(200),
    IN in_otp_code VARCHAR(10),
    IN in_otp_ref  VARCHAR(100),
    IN in_purpose  VARCHAR(50)
)
BEGIN
    SELECT '1' AS valid, user_id, user_type, email
    FROM otp_records
    WHERE email = in_email
      AND otp_code = in_otp_code
      AND otp_ref = in_otp_ref
      AND purpose = in_purpose
      AND verified = 0
      AND expires_at > NOW()
    ORDER BY id DESC
    LIMIT 1;
END$$

-- ── FIX client_password_reset (DBHandler sends @profiletype not @p_profiletype) ──
DROP PROCEDURE IF EXISTS `client_password_reset`$$
CREATE PROCEDURE `client_password_reset`(
    IN p_email       VARCHAR(200),
    IN p_password    VARCHAR(200),
    IN profiletype VARCHAR(50)
)
BEGIN
    IF profiletype = 'pharmacy' OR profiletype IS NULL THEN
        UPDATE pharmacy_users
        SET password = p_password, change_password = 0, failed_login_attempts = 0
        WHERE email = p_email AND is_deleted = 0;
    END IF;
END$$
