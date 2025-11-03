import type { Room } from "@/lib/types/room";
import type { InjectionKey, Ref } from "vue";

export const SELECT_ROOM: InjectionKey<Ref<Room | null>> = Symbol('SELECT_ROOM')