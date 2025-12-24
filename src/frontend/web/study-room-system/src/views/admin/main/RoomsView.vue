<script setup lang="ts">
import { ref, onMounted } from "vue";
import { cn } from "@/lib/utils";
import { toast } from "vue-sonner";
import type { Room,RoomEdit } from "@/lib/types/Room";
import type { Seat,SeatState } from "@/lib/types/Seat";
import { roomRequest } from "@/lib/api/roomRequest";
import { seatRequest } from "@/lib/api/seatRequest";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectGroup, SelectItem, SelectLabel } from "@/components/ui/select";
import { Eye, Edit, Trash, ClipboardList, Armchair, Loader2 } from "lucide-vue-next";

// 房间管理
// 房间列表
const rooms = ref<Room[]>([]);
const page = ref(1);
const pageSize = ref(10); // 每页显示条数
const total = ref(0);     // 总条数

// 获取房间列表
async function loadRooms() {
  try {
    const res = await roomRequest.getRooms({
      page: page.value,
      pageSize: pageSize.value
    });
    rooms.value = res.data.items;
    total.value = res.data.total;
  } catch {
    toast.error("获取房间列表失败");
  }
}
// 页面加载时获取房间列表
onMounted(loadRooms);

// 分页刷新函数
function changePage(newPage: number) {
  page.value = newPage;
  loadRooms();
}


// 添加房间
const isAddingRoom = ref(false); // 添加房间加载状态
const isAddDialogOpen = ref(false);
const newRoom = ref({
  name: "",
  openHour: 8,
  openMin: 0,
  closeHour: 22,
  closeMin: 0,
  rows: 6,
  cols: 8,
});
// 提交创建
async function handleAddRoom() {
  try {
    isAddingRoom.value = true; // 开始加载动画
    const payload = {
      name: newRoom.value.name,
      openTime: `${newRoom.value.openHour.toString().padStart(2, "0")}:${newRoom.value.openMin.toString().padStart(2, "0")}:00`,
      closeTime: `${newRoom.value.closeHour.toString().padStart(2, "0")}:${newRoom.value.closeMin.toString().padStart(2, "0")}:00`,
      rows: newRoom.value.rows,
      cols: newRoom.value.cols,
    };
    const res = await roomRequest.createRoom(payload);
    const roomId = res.data.id;

    toast.success("房间创建成功");
    isAddDialogOpen.value = false;

    const roomsRes = await roomRequest.getRooms({ page: 1, pageSize: 20 });
    rooms.value = roomsRes.data.items;

  } catch {
    toast.error("房间创建失败");
  } finally {
    isAddingRoom.value = false; // 结束加载动画
  }
}

// 查看房间详情
const selectedRoom = ref<Room | null>(null);
const selectedRoomSeats = ref<SeatState[]>([]);

async function viewRoom(room: Room) {
  if (selectedRoom.value?.id === room.id) {
    // 如果当前已展开的是同一个房间，再次点击则收起
    selectedRoom.value = null;
    selectedRoomSeats.value = [];
    return;
  }
  try {
    const res = await roomRequest.getRoom(room.id);
    selectedRoom.value = res.data;
    selectedRoomSeats.value = toSeatStateArray(res.data);
  } catch {
    toast.error("获取房间详情失败");
  }
}

// 修改房间
const editRoom = ref<RoomEdit | null>(null);
const isEditDialogOpen = ref(false);
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
// 提交修改
const isSavingEditRoom = ref(false); // 房间修改状态量
async function handleEditRoom() {
  if (!editRoom.value) return; // 判空
  try {
    isSavingEditRoom.value = true; // 开始加载动画
    const toTimeString = (hour: number, min: number) =>
      `${hour.toString().padStart(2, "0")}:${min.toString().padStart(2, "0")}:00`;

    const payload = {
      id: editRoom.value.id,
      name: editRoom.value.name,
      openTime: toTimeString(editRoom.value.openHour, editRoom.value.openMin),
      closeTime: toTimeString(editRoom.value.closeHour, editRoom.value.closeMin),
      rows: editRoom.value.rows,
      cols: editRoom.value.cols,
    };

    await roomRequest.updateRoom(payload);
    toast.success("房间修改成功");
    isEditDialogOpen.value = false;

    const res = await roomRequest.getRooms({ page: 1, pageSize: 20 });
    rooms.value = res.data.items;
  } catch {
    toast.error("房间修改失败");
  } finally {
    isSavingEditRoom.value = false; // 结束加载动画
  }
}
// 确认修改弹窗
const isConfirmDialogOpen = ref(false);
// 自定义确认修改房间弹窗
function openConfirmDialog() {
  isConfirmDialogOpen.value = true;
}
async function confirmEditRoom() {
  await handleEditRoom();
  isConfirmDialogOpen.value = false;
}

// 删除房间
const isDeleteDialogOpen = ref(false); // 控制删除确认弹窗
const pendingDeleteRoomId = ref<string | null>(null); // 记录待删除的房间 ID
const deletingRoomId = ref<string | null>(null); // 当前正在删除的房间 ID
// 打开删除确认弹窗，记录删除id
function openDeleteDialog(id: string) {
  pendingDeleteRoomId.value = id;
  isDeleteDialogOpen.value = true;
}
// 确认删除房间
async function confirmDeleteRoom() {
  if (!pendingDeleteRoomId.value) return;
  deletingRoomId.value = pendingDeleteRoomId.value; // 设置加载状态
  try {
    await roomRequest.deleteRoom(pendingDeleteRoomId.value);
    toast.success("房间已删除");
    rooms.value = rooms.value.filter(r => r.id !== pendingDeleteRoomId.value);
  } catch {
    toast.error("删除房间失败");
  } finally {
    isDeleteDialogOpen.value = false;
    deletingRoomId.value = null; // 清除加载状态
    pendingDeleteRoomId.value = null;
  }
}



// 座位管理
// 当前房间的座位状态
const currentRoomSeats = ref<SeatState[]>([]);
// 当前正在管理的房间 ID
const currentRoomId = ref<string | null>(null);
// 控制座位管理弹窗（暂未使用）
const isSeatDialogOpen = ref(false);
//切换座位管理展开/收起状态
function toggleSeatManagement(room: any) {
  if (currentRoomId.value === room.id) {
    currentRoomId.value = null; // 再次点击收起
  } else {
    currentRoomId.value = room.id;
    loadSeats(room.id);
  }
}

// 将后端 Room 数据转换为 SeatState 数组，用于前端渲染和交互
function toSeatStateArray(room: Room): SeatState[] {
  const seats: SeatState[] = [];
  for (let i = 0; i < room.rows; i++) {
    for (let j = 0; j < room.cols; j++) {
      const match = room.seats?.find(s => s.row === i && s.col === j);
      seats.push({
        row: i,
        col: j,
        id: match?.id ?? null,
        open: !!match
      });
    }
  }
  return seats;
}

// 加载指定房间的座位数据，并转换为 SeatState 格式
async function loadSeats(roomId: string) {
  try {
    const res = await roomRequest.getRoom(roomId);
    currentRoomSeats.value = toSeatStateArray(res.data);
  } catch {
    toast.error("加载座位数据失败");
  }
}

// 切换某个座位的启用状态（不立即保存）
function toggleSeat(index: number) {
  const seat = currentRoomSeats.value[index];
  if (!seat) return; // 防止越界
  seat.open = !seat.open;
}

// 保存当前座位设置
const isSavingSeats = ref(false); // 保存按钮加载状态
async function saveSeats() {
  try {

    isSavingSeats.value = true; // 开始加载动画

    // 创建座位
    const createTasks = currentRoomSeats.value
      .filter(seat => seat.open && !seat.id)
      .map(async seat => {
        const res = await seatRequest.createSeat({
          roomId: currentRoomId.value!,
          row: seat.row,
          col: seat.col
        });
        seat.id = res.data.id;
      });
    // 删除座位
    const deleteTasks = currentRoomSeats.value
      .filter(seat => !seat.open && seat.id)
      .map(seat => seatRequest.deleteSeat(seat.id!).then(() => {
        seat.id = null;
      }));

    await Promise.all([...createTasks, ...deleteTasks]);

    toast.success("座位设置已保存");
    await loadSeats(currentRoomId.value!);
  } catch {
    toast.error("保存座位设置失败");
  } finally {
    isSavingSeats.value = false; // 结束加载动画
  }
}
</script>

<template>
  <div>
    <h2 class="text-xl font-bold mb-4">房间管理</h2>
    <div class="flex justify-end mb-4">
      <Button class="hover:brightness-110" @click="isAddDialogOpen = true">添加房间</Button>
    </div>

    <div class="overflow-x-auto overflow-y-auto max-h-[75vh] border border-gray-300 rounded-lg relative">
      <table class="w-full  border-separate border-spacing-0">
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
                <Button class="bg-green-500 hover:bg-green-500 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                        @click="viewRoom(room)">
                  <Eye class="size-4" /> 查看
                </Button>
                <Button class="bg-yellow-500 hover:bg-yellow-500 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                        @click="openEdit(room)">
                  <Edit class="size-4" /> 编辑
                </Button>
                <Button class="bg-red-600 hover:bg-red-600 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                        @click="openDeleteDialog(room.id)">
                  <Trash class="size-4" />删除
                </Button>
                <Button class="bg-primary hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
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
                        <Button class="bg-primary hover:brightness-110 text-white px-4 py-2 rounded"
                                @click="saveSeats">
                          <Loader2 v-if="isSavingSeats" class="size-4 animate-spin mr-2" />{{ isSavingSeats ? '保存中...' : '保存座位设置' }}
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
    </div>

    <!-- 分页控件 -->
    <div class="flex justify-between items-center mt-4">
      <Button
        variant="default"
        size="sm"
        class="bg-primary text-white hover:bg-primary/80"
        :disabled="page <= 1"
        @click="changePage(page - 1)"
      >
        上一页
      </Button>
      <span class="text-sm text-muted-foreground">
        第 {{ page }} 页 / 共 {{ Math.ceil(total / pageSize) }} 页
      </span>
      <Button
        variant="default"
        size="sm"
        class="bg-primary text-white hover:bg-primary/80"
        :disabled="page >= Math.ceil(total / pageSize)"
        @click="changePage(page + 1)"
      >
        下一页
      </Button>
    </div>

    <!-- 房间管理 -->
    <!-- 查看房间详情 -->
    <transition name="slide-fade">
      <div v-if="selectedRoom" class="mt-6 p-4 border rounded bg-muted slide-fade-transition">
        <h3 class="text-lg font-bold mb-2">房间详情：{{ selectedRoom.name }}</h3>
        <p class="mb-2 text-sm text-muted-foreground">
          座位布局（{{ selectedRoom.rows }} x {{ selectedRoom.cols }}）
        </p>
      
        <!-- 座位网格，带最大高度和滚动条 -->
        <div class="max-h-96 overflow-y-auto border rounded p-2 mt-2">
          <div :class="cn('grid gap-1')"
                :style="{ 'grid-template-columns': `repeat(${selectedRoom.cols},1fr)` }">
            <div v-for="(seat, i) in selectedRoomSeats" :key="i">
              <Armchair
                class="size-12 transition-colors ease-in-out"
                :class="seat.open ? 'text-green-500' : 'text-gray-400'"
              />
            </div>
          </div>
        </div>
      
        <!-- 图例说明 -->
        <div class="mt-4 flex gap-x-6 text-sm text-muted-foreground">
          <div class="flex items-center gap-x-2">
            <Armchair class="size-6 text-gray-400" /> 未开放
          </div>
          <div class="flex items-center gap-x-2">
            <Armchair class="size-6 text-green-500" /> 已开放
          </div>
          <!-- 未来预约状态 -->
          <div class="flex items-center gap-x-2">
            <Armchair class="size-6 text-red-500" /> 已预约
          </div>
        </div>
      </div>
    </transition>

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
        </div>
        <DialogFooter>
          <Button class="hover:brightness-90" variant="secondary" @click="isConfirmDialogOpen = false">取消</Button>
          <Button class="bg-primary hover:brightness-90 text-white flex items-center gap-x-2" 
                  :disabled="isAddingRoom"
                  @click="handleAddRoom">
            <Loader2 v-if="isAddingRoom" class="size-4 animate-spin" />
            {{ isAddingRoom ? '创建中...' : '创建' }}
          </Button>
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
          <Button class="hover:brightness-110" @click="openConfirmDialog">保存</Button>
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
          <Button class="hover:brightness-90" variant="secondary" @click="isConfirmDialogOpen = false">取消</Button>
          <Button class="bg-primary hover:brightness-90 text-white flex items-center gap-x-2"
                  :disabled="isSavingEditRoom"
                  @click="confirmEditRoom">
            <Loader2 v-if="isSavingEditRoom" class="size-4 animate-spin" />
            {{ isSavingEditRoom ? '保存中...' : '确定' }}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
    <!-- 删除确认弹窗 -->
    <Dialog v-model:open="isDeleteDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>确认删除</DialogTitle>
        </DialogHeader>
        <p class="text-sm text-muted-foreground">确定要删除这个房间吗？此操作不可恢复。</p>
        <DialogFooter>
          <Button class="hover:brightness-90" variant="secondary" @click="isDeleteDialogOpen = false">取消</Button>
          <Button class="bg-red-600 hover:bg-red-600 hover:brightness-90 text-white flex items-center gap-x-2"
              :disabled="deletingRoomId === pendingDeleteRoomId"@click="confirmDeleteRoom">
            <Loader2 v-if="deletingRoomId === pendingDeleteRoomId" class="size-4 animate-spin" />
            {{ deletingRoomId === pendingDeleteRoomId ? '删除中...' : '确认删除' }}
          </Button>
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