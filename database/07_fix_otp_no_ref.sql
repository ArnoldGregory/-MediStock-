-- ============================================================
-- FIX: Remove otp_ref from verify SP (align with Riziki pattern)
-- Run this on the MediStock database after 06_fix_login_sps.sql
-- ============================================================

DELIMITER $$

-- FIX riziki_verify_otp — no more otp_ref, match by email + otp + purpose
DROP PROCEDURE IF EXISTS `riziki_verify_otp`$$
CREATE PROCEDURE `riziki_verify_otp`(
    IN in_email    VARCHAR(200),
    IN in_otp_code VARCHAR(10),
    IN in_purpose  VARCHAR(50)
)
BEGIN
    SELECT '1' AS valid, user_id, user_type, email
    FROM otp_records
    WHERE email = in_email
      AND otp_code = in_otp_code
      AND purpose = in_purpose
      AND verified = 0
      AND expires_at > NOW()
    ORDER BY id DESC
    LIMIT 1;
END$$

DELIMITER ;
