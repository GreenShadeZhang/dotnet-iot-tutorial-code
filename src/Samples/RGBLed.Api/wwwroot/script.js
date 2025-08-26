// API 基础URL
const API_BASE = '/api/led';

// 预定义颜色
const COLORS = {
    'Red': { R: 255, G: 0, B: 0 },
    'Green': { R: 0, G: 255, B: 0 },
    'Blue': { R: 0, G: 0, B: 255 },
    'Yellow': { R: 255, G: 255, B: 0 },
    'Cyan': { R: 0, G: 255, B: 255 },
    'Magenta': { R: 255, G: 0, B: 255 },
    'White': { R: 255, G: 255, B: 255 },
    'Orange': { R: 255, G: 165, B: 0 },
    'Purple': { R: 128, G: 0, B: 128 }
};

// 当前选择的颜色
let currentColor = { R: 255, G: 255, B: 255 };

// 初始化页面
document.addEventListener('DOMContentLoaded', function() {
    initializeColorGrid();
    initializeSliders();
    refreshStatus();
    
    // 定期刷新状态
    setInterval(refreshStatus, 5000);
});

// 初始化颜色网格
function initializeColorGrid() {
    const colorGrid = document.getElementById('colorGrid');
    
    Object.entries(COLORS).forEach(([name, color]) => {
        const button = document.createElement('button');
        button.className = 'btn btn-color';
        button.style.background = `rgb(${color.R}, ${color.G}, ${color.B})`;
        button.onclick = () => selectColor(name, color);
        
        const span = document.createElement('span');
        span.textContent = name;
        button.appendChild(span);
        
        colorGrid.appendChild(button);
    });
}

// 初始化滑块
function initializeSliders() {
    const brightnessSlider = document.getElementById('brightnessSlider');
    const speedSlider = document.getElementById('speedSlider');
    
    brightnessSlider.addEventListener('input', function() {
        document.getElementById('brightnessValue').textContent = this.value + '%';
    });
    
    speedSlider.addEventListener('input', function() {
        document.getElementById('speedValue').textContent = this.value + 'ms';
    });
}

// 显示消息
function showMessage(text, type = 'success') {
    const messageEl = document.getElementById('message');
    messageEl.textContent = text;
    messageEl.className = `message ${type}`;
    messageEl.style.display = 'block';
    
    setTimeout(() => {
        messageEl.style.display = 'none';
    }, 3000);
}

// API 请求帮助函数
async function apiRequest(endpoint, method = 'GET', data = null) {
    try {
        const options = {
            method,
            headers: {
                'Content-Type': 'application/json',
            }
        };
        
        if (data) {
            options.body = JSON.stringify(data);
        }
        
        const response = await fetch(API_BASE + endpoint, options);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('API请求失败:', error);
        showMessage(`请求失败: ${error.message}`, 'error');
        throw error;
    }
}

// 刷新状态
async function refreshStatus() {
    try {
        const status = await apiRequest('/status');
        updateStatusDisplay(status);
    } catch (error) {
        console.error('刷新状态失败:', error);
    }
}

// 更新状态显示
function updateStatusDisplay(status) {
    document.getElementById('currentEffect').textContent = status.currentEffect || '-';
    document.getElementById('currentBrightness').textContent = status.brightness + '%';
    document.getElementById('currentSpeed').textContent = status.speed + 'ms';
    
    // 更新LED预览
    const ledPreview = document.getElementById('ledPreview');
    const color = status.currentColor;
    const brightness = status.brightness / 100;
    
    if (status.currentEffect === 'Rainbow') {
        // 彩虹效果显示渐变
        ledPreview.style.background = 'conic-gradient(from 0deg, red, yellow, green, cyan, blue, magenta, red)';
        ledPreview.style.animation = 'rainbow 2s linear infinite';
    } else if (status.currentEffect === 'Blink') {
        // 闪烁效果
        ledPreview.style.background = `rgb(${color.r * brightness}, ${color.g * brightness}, ${color.b * brightness})`;
        ledPreview.style.animation = 'blink 1s ease-in-out infinite';
    } else if (status.currentEffect === 'Breathe') {
        // 呼吸效果
        ledPreview.style.background = `rgb(${color.r * brightness}, ${color.g * brightness}, ${color.b * brightness})`;
        ledPreview.style.animation = 'breathe 2s ease-in-out infinite';
    } else {
        // 静态或关闭
        ledPreview.style.animation = 'none';
        ledPreview.style.background = `rgb(${color.r * brightness}, ${color.g * brightness}, ${color.b * brightness})`;
    }
    
    // 添加动画样式
    if (!document.getElementById('animationStyles')) {
        const style = document.createElement('style');
        style.id = 'animationStyles';
        style.textContent = `
            @keyframes rainbow {
                0% { filter: hue-rotate(0deg); }
                100% { filter: hue-rotate(360deg); }
            }
            @keyframes blink {
                0%, 50% { opacity: 1; }
                51%, 100% { opacity: 0.3; }
            }
            @keyframes breathe {
                0%, 100% { opacity: 0.3; }
                50% { opacity: 1; }
            }
        `;
        document.head.appendChild(style);
    }
}

// 选择颜色
function selectColor(name, color) {
    currentColor = color;
    showMessage(`已选择颜色: ${name}`);
    
    // 高亮选中的颜色按钮
    document.querySelectorAll('.btn-color').forEach(btn => {
        btn.style.transform = '';
        btn.style.boxShadow = '';
    });
    
    event.target.closest('.btn-color').style.transform = 'scale(1.1)';
    event.target.closest('.btn-color').style.boxShadow = '0 0 20px rgba(255,255,255,0.6)';
}

// 设置效果
async function setEffect(effectName) {
    try {
        const brightness = parseInt(document.getElementById('brightnessSlider').value);
        const speed = parseInt(document.getElementById('speedSlider').value);
        
        const request = {
            Effect: effectName,
            Color: currentColor,
            Brightness: brightness,
            Speed: speed
        };
        
        await apiRequest('/effect', 'POST', request);
        showMessage(`${effectName} 效果已设置`);
        
        // 延迟刷新状态以看到变化
        setTimeout(refreshStatus, 500);
        
    } catch (error) {
        console.error('设置效果失败:', error);
    }
}

// 应用设置
async function applySettings() {
    try {
        const brightness = parseInt(document.getElementById('brightnessSlider').value);
        const speed = parseInt(document.getElementById('speedSlider').value);
        
        const request = {
            Effect: 'Static', // 默认使用静态效果来应用设置
            Color: currentColor,
            Brightness: brightness,
            Speed: speed
        };
        
        await apiRequest('/effect', 'POST', request);
        showMessage('设置已应用');
        
        setTimeout(refreshStatus, 500);
        
    } catch (error) {
        console.error('应用设置失败:', error);
    }
}

// 快速设置预定义效果
async function quickSetBlink() {
    await setQuickEffect('Blink', currentColor, 1000);
}

async function quickSetBreathe() {
    await setQuickEffect('Breathe', currentColor, 2000);
}

async function quickSetRainbow() {
    await setQuickEffect('Rainbow', { Red: 255, Green: 255, Blue: 255 }, 3600);
}

async function setQuickEffect(effect, color, speed) {
    try {
        const brightness = parseInt(document.getElementById('brightnessSlider').value);
        
        const request = {
            Effect: effect,
            Color: color,
            Brightness: brightness,
            Speed: speed
        };
        
        await apiRequest('/effect', 'POST', request);
        showMessage(`${effect} 效果已启动`);
        
        setTimeout(refreshStatus, 500);
        
    } catch (error) {
        console.error('设置快速效果失败:', error);
    }
}

// 键盘快捷键
document.addEventListener('keydown', function(event) {
    switch(event.key) {
        case '0':
            setEffect('Off');
            break;
        case '1':
            setEffect('Static');
            break;
        case '2':
            setEffect('Blink');
            break;
        case '3':
            setEffect('Breathe');
            break;
        case '4':
            setEffect('Rainbow');
            break;
        case 'r':
            selectColor('Red', COLORS.Red);
            break;
        case 'g':
            selectColor('Green', COLORS.Green);
            break;
        case 'b':
            selectColor('Blue', COLORS.Blue);
            break;
        case 'w':
            selectColor('White', COLORS.White);
            break;
        case ' ':
            event.preventDefault();
            refreshStatus();
            break;
    }
});

// 触摸设备支持
document.addEventListener('touchstart', function() {}, { passive: true });
