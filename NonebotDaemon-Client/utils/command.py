from enum import Enum

class Command(Enum):
	ClientHeartbeat = 1,
	ClientNoResponse = 2,
	Null = 255