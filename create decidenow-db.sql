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
);

CREATE TABLE follow (
  fromid varchar(36) NOT NULL,
  toid varchar(36) NOT NULL,
  totype enum('user','group') NOT NULL
);

CREATE TABLE invite (
  userid varchar(36) NOT NULL,
  inviteto varchar(36) NOT NULL,
  invitetype enum('user','group') DEFAULT NULL
);

CREATE TABLE poll (
  pollid varchar(36) NOT NULL,
  userid varchar(36) DEFAULT NULL,
  PRIMARY KEY (pollid),
  KEY userid (userid),
  CONSTRAINT poll_ibfk_1 FOREIGN KEY (userid) REFERENCES user (userid)
);

CREATE TABLE polldata (
  pollid varchar(36) DEFAULT NULL,
  dataindex tinyint(4) DEFAULT NULL,
  data longblob,
  size int default 0,
  KEY pollid (pollid),
  CONSTRAINT polldata_ibfk_1 FOREIGN KEY (pollid) REFERENCES poll (pollid)
);

CREATE TABLE pollsetting (
  pollid varchar(36) DEFAULT NULL,
  setting varchar(50) DEFAULT NULL,
  value varchar(50) DEFAULT NULL,
  KEY pollid (pollid),
  CONSTRAINT pollsetting_ibfk_1 FOREIGN KEY (pollid) REFERENCES poll (pollid)
);

create table profileimage(
	userid varchar(36) not null,
    image longblob,
    size int default 0
);

create table code(
  userid varchar(36) not null,
  code varchar(6) not null,
  guesses int default 0,
  timestamp bigint(20),
  KEY userid (userid),
  CONSTRAINT code_ibfk_1 FOREIGN KEY (userid) REFERENCES user (userid)
);

create table securitytoken(
  userid varchar(36) not null,
  token varchar(36) not null,
  timestamp bigint(20),
  KEY userid (userid),
  CONSTRAINT securitytoken_ibfk_1 FOREIGN KEY (userid) REFERENCES user (userid)
);
