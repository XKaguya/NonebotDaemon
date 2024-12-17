import websockets
import asyncio

from nonebot import on_command, require, get_bot, get_driver
from nonebot.log import logger
from zhenxun.configs.config import Config

from .utils.command import Command
from .utils.init import InitConfig

InitConfig()

scheduler = require("nonebot_plugin_apscheduler").scheduler

Interval = Config.get_config("NonebotDaemon", "INTERVAL")
websocket_uri = Config.get_config("NonebotDaemon", "WebSocket")
Websocket = None

driver = get_driver()

async def initialize_websocket():
	"""尝试连接 WebSocket"""
	global Websocket
	try:
		Websocket = await websockets.connect(websocket_uri, max_size=50000)
		logger.info("WebSocket connected successfully.")
	except Exception as ex:
		logger.error(f"Failed to connect to WebSocket: {ex}")

@driver.on_startup
async def Initialize():
	"""在启动时初始化 WebSocket 连接"""
	await initialize_websocket()

async def check_websocket():
	"""检查 WebSocket 是否仍然连接"""
	try:
		if Websocket is not None and Websocket.open:
			return True
		else:
			return False
	except Exception as ex:
		logger.error(f"Error checking WebSocket connection: {ex}")
		return False

async def SendAndReceive(message):
	"""发送消息并接收响应，包含重连机制"""
	try:
		if Websocket is not None and await check_websocket():
			await Websocket.send(message)
		else:
			logger.warning("WebSocket is not connected, attempting to reconnect...")
			await initialize_websocket()
			if Websocket is not None:
				await Websocket.send(message)
			else:
				logger.error("Failed to reconnect to WebSocket.")
				return None
	except Exception as ex:
		logger.error(f"An error occurred while connecting to the websocket server: {ex}")
		return None

@scheduler.scheduled_job("interval", seconds=Interval, id="HeartbeatFunc", max_instances=1)
async def HeartbeatFunc():
	"""每 Interval 秒发送一次心跳消息"""

	try:
		message = f"{Command.ClientHeartbeat.name}"
		await asyncio.wait_for(SendAndReceive(message), timeout=40)
	except Exception as ex:
		logger.error(f"An error occurred while connecting to the websocket server: {ex}")
