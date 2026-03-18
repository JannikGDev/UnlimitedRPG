// Types mirroring the API DTOs

export interface WorldDto {
	id: string;
	name: string;
}

export interface PlayerDto {
	name: string;
	currentHp: number;
	maxHp: number;
	attackBonus: number;
	damageBonus: number;
	armorClass: number;
}

export interface EnemyDto {
	name: string;
	currentHp: number;
	maxHp: number;
	attackBonus: number;
	damageBonus: number;
	armorClass: number;
	status: string; // "Alive" | "Staggered" | "Dead"
}

export interface CombatLogEntryDto {
	round: number;
	hit: boolean;
	damage: number;
	narration: string;
	provider: string; // "stub" | "claude" | "pending"
}

export interface SessionStateDto {
	sessionId: string;
	status: string; // "Active" | "Completed" | "Abandoned"
	round: number;
	player: PlayerDto;
	enemy: EnemyDto;
	combatLog: CombatLogEntryDto[];
}

// API calls

async function request<T>(path: string, init?: RequestInit): Promise<T> {
	const res = await fetch(path, {
		headers: { 'Content-Type': 'application/json' },
		...init
	});
	if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
	return res.json() as Promise<T>;
}

export function getWorlds(): Promise<WorldDto[]> {
	return request('/api/worlds');
}

export function createSession(worldId: string, playerName: string): Promise<SessionStateDto> {
	return request('/api/sessions', {
		method: 'POST',
		body: JSON.stringify({ worldId, playerName })
	});
}

export function getSession(id: string): Promise<SessionStateDto> {
	return request(`/api/sessions/${id}`);
}

export function executeAction(id: string, type: string): Promise<SessionStateDto> {
	return request(`/api/sessions/${id}/actions`, {
		method: 'POST',
		body: JSON.stringify({ type })
	});
}
