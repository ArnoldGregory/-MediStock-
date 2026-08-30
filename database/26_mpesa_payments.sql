-- ============================================================================
-- MediStock - M-Pesa STK Push (Daraja) - payments log + lifecycle procs
-- Additive, replay-safe. Lives on its own table so nothing existing is touched.
-- Procs use API-compatible p_ parameter names (MySqlConnector binds by name).
-- ============================================================================

DELIMITER $$

CREATE TABLE IF NOT EXISTS `mpesa_payments` (
  `id`                   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `pharmacy_id`          BIGINT UNSIGNED NOT NULL DEFAULT 0,
  `user_id`              BIGINT UNSIGNED NULL,
  `phone`                VARCHAR(32)     NOT NULL,
  `amount`               DECIMAL(12,2)   NOT NULL,
  `account_reference`    VARCHAR(64)     NOT NULL DEFAULT '',
  `transaction_desc`     VARCHAR(128)    NOT NULL DEFAULT '',
  `checkout_request_id`  VARCHAR(64)     NULL,
  `merchant_request_id`  VARCHAR(64)     NULL,
  `result_code`          INT             NULL,
  `result_desc`          VARCHAR(255)    NULL,
  `mpesa_receipt`        VARCHAR(64)     NULL,
  `paid_amount`          DECIMAL(12,2)   NULL,
  `status`               VARCHAR(32)     NOT NULL DEFAULT 'Initiated',
  `payload_json`         MEDIUMTEXT      NULL,
  `created_on`           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_on`           DATETIME        NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `ix_mpesa_checkout` (`checkout_request_id`),
  KEY `ix_mpesa_pharmacy` (`pharmacy_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4$$

DROP PROCEDURE IF EXISTS `mpesa_add_payment`$$

CREATE PROCEDURE `mpesa_add_payment`(
    IN p_pharmacy_id        BIGINT,
    IN p_user_id            BIGINT,
    IN p_phone              VARCHAR(32),
    IN p_amount             DECIMAL(12,2),
    IN p_account_reference  VARCHAR(64),
    IN p_transaction_desc   VARCHAR(128),
    OUT p_id                BIGINT
)
BEGIN
    INSERT INTO mpesa_payments
        (pharmacy_id, user_id, phone, amount, account_reference, transaction_desc, status)
    VALUES
        (p_pharmacy_id, p_user_id, p_phone, p_amount, p_account_reference, p_transaction_desc, 'Initiated');
    SET p_id = LAST_INSERT_ID();
END$$

DROP PROCEDURE IF EXISTS `mpesa_set_checkout`$$

CREATE PROCEDURE `mpesa_set_checkout`(
    IN p_payment_id          BIGINT,
    IN p_checkout_request_id VARCHAR(64),
    IN p_merchant_request_id VARCHAR(64),
    IN p_status              VARCHAR(32)
)
BEGIN
    UPDATE mpesa_payments
    SET checkout_request_id = p_checkout_request_id,
        merchant_request_id = p_merchant_request_id,
        status = p_status
    WHERE id = p_payment_id;
END$$

DROP PROCEDURE IF EXISTS `mpesa_update_from_callback`$$

CREATE PROCEDURE `mpesa_update_from_callback`(
    IN p_checkout_request_id VARCHAR(64),
    IN p_result_code         INT,
    IN p_result_desc         VARCHAR(255),
    IN p_mpesa_receipt       VARCHAR(64),
    IN p_paid_amount         DECIMAL(12,2)
)
BEGIN
    IF p_result_code = 0 THEN
        UPDATE mpesa_payments
        SET result_code     = p_result_code,
            result_desc     = p_result_desc,
            mpesa_receipt   = p_mpesa_receipt,
            paid_amount     = p_paid_amount,
            status          = 'Success'
        WHERE checkout_request_id = p_checkout_request_id;
    ELSE
        UPDATE mpesa_payments
        SET result_code = p_result_code,
            result_desc = p_result_desc,
            status      = 'Failed'
        WHERE checkout_request_id = p_checkout_request_id;
    END IF;
END$$

DROP PROCEDURE IF EXISTS `mpesa_get_by_checkout`$$

CREATE PROCEDURE `mpesa_get_by_checkout`(
    IN p_checkout_request_id VARCHAR(64)
)
BEGIN
    SELECT * FROM mpesa_payments WHERE checkout_request_id = p_checkout_request_id LIMIT 1;
END$$

DROP PROCEDURE IF EXISTS `mpesa_list`$$

CREATE PROCEDURE `mpesa_list`(
    IN p_pharmacy_id BIGINT
)
BEGIN
    SELECT * FROM mpesa_payments
    WHERE pharmacy_id = p_pharmacy_id
    ORDER BY id DESC LIMIT 50;
END$$

DELIMITER ;