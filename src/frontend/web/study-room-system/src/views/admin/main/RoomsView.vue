<script setup lang="ts">
import { ref, onMounted } from "vue";
import { cn } from "@/lib/utils";
import { toast } from "vue-sonner";
import { roomRequest } from "@/lib/api/roomRequest";
import { seatRequest } from "@/lib/api/seatRequest";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectGroup, SelectItem, SelectLabel } from "@/components/ui/select";
import { Eye, Edit, Trash, ClipboardList, Armchair } from "lucide-vue-next";

const rooms = ref<any[]>([]);
const selectedRoom = ref<any | null>(null);
const isAddDialogOpen = ref(false);
const isEditDialogOpen = ref(false);
const isConfirmDialogOpen = ref(false);

const newRoom = ref({
  name: "",
  openHour: 8,
  openMin: 0,
  closeHour: 22,
  closeMin: 0,
  rows: 10,
  cols: 10,
  seatCount: 0,
});
const editRoom = ref<any | null>(null);

// 座位管理
const isSeatDialogOpen = ref(false);
const currentRoomSeats = ref<any[]>([]);
const currentRoomId = ref<string | null>(null);
const editSeat = ref<any | null>(null);

// 获取房间列表
onMounted(async () => {
  try {
    const res = await roomRequest.getRooms();
    rooms.value = res.data;
  } catch {
    toast.error("获取房间列表失败");
  }
});

// 查看房间详情
async function viewRoom(room: any) {
  try {
    const res = await roomRequest.getRoom(room.id);
    selectedRoom.value = res.data;
  } catch {
    toast.error("获取房间详情失败");
  }
}

// 添加房间
async function handleAddRoom() {
  try {
    const payload = {
      name: newRoom.value.name,
      openTime: `${newRoom.value.openHour.toString().padStart(2, "0")}:${newRoom.value.openMin.toString().padStart(2, "0")}:00`,
      closeTime: `${newRoom.value.closeHour.toString().padStart(2, "0")}:${newRoom.value.closeMin.toString().padStart(2, "0")}:00`,
      rows: newRoom.value.rows,
      cols: newRoom.value.cols,
    };
    const res = await roomRequest.createRoom(payload);
    const roomId = res.data.id;

    // 根据 seatCount 创建座位
    for (let i = 0; i < newRoom.value.seatCount; i++) {
      const row = Math.floor(i / newRoom.value.cols);
      const col = i % newRoom.value.cols;
      await seatRequest.createSeat({ roomId, row, col });
    }

    toast.success("房间和座位创建成功");
    isAddDialogOpen.value = false;
    const roomsRes = await roomRequest.getRooms();
    rooms.value = roomsRes.data;
  } catch {
    toast.error("房间或座位创建失败");
  }
}

// 修改房间
async function handleEditRoom() {
  try {
    const payload = {
      id: editRoom.value.id,
      name: editRoom.value.name,
      openTime: `${editRoom.value.openHour.toString().padStart(2, "0")}:${editRoom.value.openMin.toString().padStart(2, "0")}:00`,
      closeTime: `${editRoom.value.closeHour.toString().padStart(2, "0")}:${editRoom.value.closeMin.toString().padStart(2, "0")}:00`,
      rows: editRoom.value.rows,
      cols: editRoom.value.cols,
    };
    await roomRequest.updateRoom(payload);
    toast.success("房间修改成功");
    isEditDialogOpen.value = false;
    const res = await roomRequest.getRooms();
    rooms.value = res.data;
  } catch {
    toast.error("房间修改失败");
  }
}

// 删除房间
async function handleDeleteRoom(id: string) {
  try {
    await roomRequest.deleteRoom(id);
    toast.success("房间已删除");
    rooms.value = rooms.value.filter((r) => r.id !== id);
  } catch {
    toast.error("删除房间失败");
  }
}

// 打开编辑弹窗
function openEdit(room: any) {
  const [openH, openM] = String(room.openTime).split(":");
  const [closeH, closeM] = String(room.closeTime).split(":");
  editRoom.value = {
    ...room,
    openHour: Number(openH ?? 8),
    openMin: Number(openM ?? 0),
    closeHour: Number(closeH ?? 22),
    closeMin: Number(closeM ?? 0),
  };
  isEditDialogOpen.value = true;
}

// 座位管理
function toggleSeatManagement(room: any) {
  if (currentRoomId.value === room.id) {
    currentRoomId.value = null; // 再次点击收起
  } else {
    currentRoomId.value = room.id;
    loadSeats(room.id);
  }
}

async function loadSeats(roomId: string) {
  const res = await roomRequest.getRoom(roomId);
  // 初始化座位状态：没有的设为灰色
  const seats = Array(res.data.rows * res.data.cols).fill(null).map((_, i) => {
    const row = Math.floor(i / res.data.cols);
    const col = i % res.data.cols;
    const seat = res.data.seats?.find(s => s.row === row && s.col === col);
    return { row, col, open: !!seat };
  });
  currentRoomSeats.value = seats;
}

function toggleSeat(index: number) {
  const seat = currentRoomSeats.value[index];
  seat.open = !seat.open; // 只切换状态，不直接调用 API
}

async function saveSeats() {
  try {
    for (const seat of currentRoomSeats.value) {
      if (seat.open && !seat.id) {
        await seatRequest.createSeat({ roomId: currentRoomId.value!, row: seat.row, col: seat.col });
      } else if (!seat.open && seat.id) {
        await seatRequest.deleteSeat(seat.id);
      }
    }
    toast.success("座位设置已保存");
    loadSeats(currentRoomId.value!); // 刷新状态
  } catch {
    toast.error("保存座位设置失败");
  }
}

// 自定义确认修改房间弹窗
function openConfirmDialog() {
  isConfirmDialogOpen.value = true;
}
async function confirmEditRoom() {
  await handleEditRoom();
  isConfirmDialogOpen.value = false;
}
</script>

<template>
  <div>
    <h2 class="text-xl font-bold mb-4">房间管理</h2>
    <div class="flex justify-end mb-4">
      <Button @click="isAddDialogOpen = true">添加房间</Button>
    </div>

    <table class="w-full border-collapse border border-gray-300">
      <thead>
        <tr class="bg-gray-100">
          <th class="border p-2">房间名称</th>
          <th class="border p-2">开放时间</th>
          <th class="border p-2">关闭时间</th>
          <th class="border p-2">房间大小</th>
          <th class="border p-2">座位数量</th>
          <th class="border p-2">操作</th>
        </tr>
      </thead>
      <tbody>
        <!-- 移除 transition-group，改用普通循环 -->
        <template v-for="room in rooms" :key="room.id">
          <!-- 房间信息行 -->
          <tr>
            <td class="border p-2">{{ room.name }}</td>
            <td class="border p-2">{{ room.openTime }}</td>
            <td class="border p-2">{{ room.closeTime }}</td>
            <td class="border p-2">{{ room.rows * room.cols }}</td>
            <td class="border p-2">{{ room.seats?.length || 0 }}</td>
            <td class="border p-2 flex gap-x-2">
              <!-- 原有按钮保持不变 -->
              <Button class="bg-green-500 text-white px-2 py-1 rounded flex items-center gap-x-1"
                      @click="viewRoom(room)">
                <Eye class="size-4" /> 查看
              </Button>
              <Button class="bg-yellow-500 text-white px-2 py-1 rounded flex items-center gap-x-1"
                      @click="openEdit(room)">
                <Edit class="size-4" /> 修改
              </Button>
              <Button class="bg-red-500 text-white px-2 py-1 rounded flex items-center gap-x-1"
                      @click="handleDeleteRoom(room.id)">
                <Trash class="size-4" /> 删除
              </Button>
              <Button class="bg-primary text-white px-2 py-1 rounded flex items-center gap-x-1"
                      @click="toggleSeatManagement(room)">
                <ClipboardList class="size-4" /> 座位管理
              </Button>
            </td>
          </tr>
        
          <!-- 重构展开行：使用 transition 包裹，优化动画触发 -->
          <tr>
            <td colspan="6" class="p-0 border">
              <!-- 核心修改：用 transition 包裹，添加动画容器类 -->
              <transition name="slide-fade">
                <div 
                  v-show="room.id === currentRoomId" 
                  class="slide-fade-transition"
                >
                  <div class="p-4 bg-muted">
                    <h3 class="text-lg font-bold mb-2">座位管理 - {{ room.name }}</h3>
                    <p>
                      最大座位数：{{ room.rows * room.cols }}，
                      已开放座位数：{{ currentRoomSeats.filter(s => s.open).length }}
                    </p>
                    <p class="text-sm text-muted-foreground mt-2">
                      修改座位状态后请点击“保存”按钮，否则不会生效
                    </p>
                  
                    <!-- 座位区域加滚动条 -->
                    <div class="max-h-96 overflow-y-auto border rounded p-2 mt-2">
                      <div :class="cn('grid gap-1')"
                           :style="{ 'grid-template-columns': `repeat(${room.cols},1fr)` }">
                        <div v-for="(seat, i) in currentRoomSeats" :key="i">
                          <Armchair
                            class="size-12 cursor-pointer transition-colors ease-in-out"
                            :class="seat.open ? 'text-green-500' : 'text-gray-400'"
                            @click="toggleSeat(i)"
                          />
                        </div>
                      </div>
                    </div>
                  
                    <!-- 保存按钮 -->
                    <div class="mt-4 flex justify-end">
                      <Button class="bg-primary text-white px-4 py-2 rounded"
                              @click="saveSeats">
                        保存座位设置
                      </Button>
                    </div>
                  </div>
                </div>
              </transition>
            </td>
          </tr>
        </template>
      </tbody>
    </table>

    <!-- 查看房间详情 -->
    <div v-if="selectedRoom" class="mt-6 p-4 border rounded bg-muted">
      <h3 class="text-lg font-bold mb-2">房间详情：{{ selectedRoom.name }}</h3>
      <p class="mb-2 text-sm text-muted-foreground">座位布局（{{ selectedRoom.rows }} x {{ selectedRoom.cols }}）：</p>
      <div class="grid gap-1" :style="`grid-template-columns: repeat(${selectedRoom.cols}, minmax(20px, 1fr))`">
        <div
          v-for="seat in selectedRoom.seats"
          :key="seat.id"
          class="bg-primary text-white text-xs p-1 text-center rounded"
        >
          {{ seat.row }}-{{ seat.col }}
        </div>
      </div>
    </div>

    <!-- 添加房间弹窗 -->
    <Dialog v-model:open="isAddDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>添加房间</DialogTitle>
        </DialogHeader>
        <div class="flex flex-col gap-y-3">
          <Input v-model="newRoom.name" placeholder="房间名称" />
          <!-- 开放时间 -->
          <div class="flex items-center gap-x-2">
            <span class="text-sm text-black">开放时间</span>
            <Select v-model="newRoom.openHour">
              <SelectTrigger><SelectValue placeholder="时" /></SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>时</SelectLabel>
                  <SelectItem v-for="v in Array(24).fill(0).map((_, i) => i)" :key="v" :value="v">{{ v }}</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
            <span>时</span>
            <Select v-model="newRoom.openMin">
              <SelectTrigger><SelectValue placeholder="分" /></SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>分</SelectLabel>
                  <SelectItem v-for="v in Array(12).fill(0).map((_, i) => i*5)" :key="v" :value="v">{{ v.toString().padStart(2,'0') }}</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
            <span>分</span>
          </div>
        
          <!-- 截至时间 -->
          <div class="flex items-center gap-x-2">
            <span class="text-sm text-black">截至时间</span>
            <Select v-model="newRoom.closeHour">
              <SelectTrigger><SelectValue placeholder="时" /></SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>时</SelectLabel>
                  <SelectItem v-for="v in Array(24).fill(0).map((_, i) => i)" :key="v" :value="v">{{ v }}</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
            <span>时</span>
            <Select v-model="newRoom.closeMin">
              <SelectTrigger><SelectValue placeholder="分" /></SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>分</SelectLabel>
                  <SelectItem v-for="v in Array(12).fill(0).map((_, i) => i*5)" :key="v" :value="v">{{ v.toString().padStart(2,'0') }}</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
            <span>分</span>
          </div>
        
          <!-- 房间大小 -->
          <div class="flex items-center gap-x-2">
            <span class="text-sm text-black">房间大小</span>
            <Input v-model="newRoom.rows" type="number" placeholder="row" class="w-24" />
            <Input v-model="newRoom.cols" type="number" placeholder="col" class="w-24" />
          </div>  
          <!-- 座位数量 -->
          <div class="flex items-center gap-x-2 mt-2">
            <span class="text-sm text-black">座位数量</span>
            <Input v-model="newRoom.seatCount" type="number" placeholder="请输入座位数量" class="w-36" />
          </div>
        </div>
        <DialogFooter>
          <Button @click="handleAddRoom">确认</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>  
    <!-- 修改房间弹窗 -->
    <Dialog v-model:open="isEditDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>修改房间</DialogTitle>
        </DialogHeader>
        <div v-if="editRoom" class="flex flex-col gap-y-3">
          <!-- 房间名称 -->
          <div class="flex items-center gap-x-2">
            <span class="text-sm text-black">房间名称</span>
            <Input v-model="editRoom.name" placeholder="请输入房间名称" class="w-48" />
          </div>
          <!-- 开放时间 -->
          <div class="flex items-center gap-x-2">
            <span class="text-sm text-black">开放时间</span>
            <Select v-model="editRoom.openHour">
              <SelectTrigger><SelectValue placeholder="时" /></SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>时</SelectLabel>
                  <SelectItem v-for="v in Array(24).fill(0).map((_, i) => i)" :key="v" :value="v">{{ v }}</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
            <span>时</span>
            <Select v-model="editRoom.openMin">
              <SelectTrigger><SelectValue placeholder="分" /></SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>分</SelectLabel>
                  <SelectItem v-for="v in Array(12).fill(0).map((_, i) => i*5)" :key="v" :value="v">{{ v.toString().padStart(2,'0') }}</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
            <span>分</span>
          </div>
          <!-- 截至时间 -->
          <div class="flex items-center gap-x-2">
            <span class="text-sm text-black">截至时间</span>
            <Select v-model="editRoom.closeHour">
              <SelectTrigger><SelectValue placeholder="时" /></SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>时</SelectLabel>
                  <SelectItem v-for="v in Array(24).fill(0).map((_, i) => i)" :key="v" :value="v">{{ v }}</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
            <span>时</span>
            <Select v-model="editRoom.closeMin">
              <SelectTrigger><SelectValue placeholder="分" /></SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>分</SelectLabel>
                  <SelectItem v-for="v in Array(12).fill(0).map((_, i) => i*5)" :key="v" :value="v">{{ v.toString().padStart(2,'0') }}</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
            <span>分</span>
          </div>
          <!-- 房间大小 -->
          <div class="flex items-center gap-x-2">
            <span class="text-sm text-black">房间大小</span>
            <Input v-model="editRoom.rows" type="number" placeholder="row (行数)" class="w-24" />
            <Input v-model="editRoom.cols" type="number" placeholder="col (列数)" class="w-24" />
          </div>
        </div>
        <DialogFooter>
          <Button @click="openConfirmDialog">保存</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- 确认弹窗（独立出来） -->
    <Dialog v-model:open="isConfirmDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>确认保存</DialogTitle>
        </DialogHeader>
        <p class="text-sm text-muted-foreground">确定要保存修改吗？</p>
        <DialogFooter>
          <Button variant="secondary" @click="isConfirmDialogOpen = false">取消</Button>
          <Button class="bg-primary text-white" @click="confirmEditRoom">确认</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- 座位管理弹窗 -->
    <Dialog v-model:open="isSeatDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>座位管理</DialogTitle>
        </DialogHeader>
      
        <!-- 座位列表 -->
        <div class="grid gap-1" :style="`grid-template-columns: repeat(${selectedRoom?.cols || 0}, minmax(20px, 1fr))`">
          <div
            v-for="seat in currentRoomSeats"
            :key="seat.id"
            class="border p-2 text-center rounded"
          >
            {{ seat.row }}-{{ seat.col }}
          </div>
        </div>
      
        <DialogFooter>
          <Button variant="secondary" @click="isSeatDialogOpen = false">关闭</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  </div>
</template>

<!-- 下拉动画样式 -->
<style scoped>
.slide-fade-enter-from,
.slide-fade-leave-to {
  max-height: 0;
  opacity: 0;
  overflow: hidden;
}

.slide-fade-enter-active,
.slide-fade-leave-active {
  transition: all 0.3s ease;
}

.slide-fade-enter-to,
.slide-fade-leave-from {
  max-height: 1000px; /* 足够大的值容纳内容 */
  opacity: 1;
}

/* 防止动画过程中内容溢出 */
.slide-fade-transition {
  overflow: hidden;
  transition: max-height 0.3s ease, opacity 0.3s ease;
}
</style>