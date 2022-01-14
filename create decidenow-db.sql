CREATE TABLE user (
  userid varchar(36) NOT NULL,
  username varchar(200) NOT NULL,
  email varchar(254) NOT NULL,
  createtime bigint(20) DEFAULT NULL,
  PRIMARY KEY (userid),
  UNIQUE KEY username (username),
  UNIQUE KEY email (email)
);

CREATE TABLE password (
  userid varchar(36) NOT NULL,
  password varchar(64) NOT NULL,
  KEY userid (userid),
  CONSTRAINT password_ibfk_1 FOREIGN KEY (userid) REFERENCES user (userid) ON DELETE CASCADE
);

CREATE TABLE challenge (
  userid varchar(36) DEFAULT NULL,
  challenge varchar(64) DEFAULT NULL,
  solved varchar(64) DEFAULT NULL,
  createtime bigint(20) DEFAULT NULL,
  KEY userid (userid),
  CONSTRAINT challenge_ibfk_1 FOREIGN KEY (userid) REFERENCES user (userid) ON DELETE CASCADE
);

CREATE TABLE token (
  tokenid varchar(36) NOT NULL,
  userid varchar(36) DEFAULT NULL,
  clientip varchar(50) DEFAULT NULL,
  lastused bigint(20) DEFAULT NULL,
  createtime bigint(20) DEFAULT NULL,
  PRIMARY KEY (tokenid),
  KEY userid (userid),
  CONSTRAINT token_ibfk_1 FOREIGN KEY (userid) REFERENCES user (userid)
);

CREATE TABLE usergroup (
  groupid varchar(36) NOT NULL,
  admin varchar(36) DEFAULT NULL,
  visibility enum('public','private') NOT NULL,
  name varchar(100) NOT NULL,
  PRIMARY KEY (groupid)
) 

