from zhenxun.configs.config import Config
from nonebot.log import logger

def InitConfig():
	Config.add_plugin_config(
		"NonebotDaemon",
		"INTERVAL",
		10,
		help="心跳发送间隔",
		default_value=10,
		type=int,
	)

	Config.add_plugin_config(
		"NonebotDaemon",
		"WebSocket",
		"ws://localhost:8890",
		help="WebSocket服务器地址",
		default_value="ws://localhost:8890",
		type=str,
	)
	
	logger.success("Config registered.")