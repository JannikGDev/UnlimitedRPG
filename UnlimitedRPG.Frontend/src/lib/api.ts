export interface CharacterDto {
	id: string;
	name: string;
	description: string;
}

export interface SessionDto {
	id: string;
	characterId: string;
	startedAt: string;
	status: string;
}

export interface ActiveSessionDto {
	id: string;
	characterId: string;
	characterName: string;
	startedAt: string;
}

export interface SessionMessageDto {
	id: string;
	mode: string;
	text: string;
	sentAt: string;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
	const res = await fetch(path, {
		headers: { 'Content-Type': 'application/json' },
		...init
	});
	if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
	return res.json() as Promise<T>;
}

export function getCharacters(): Promise<CharacterDto[]> {
	return request('/api/characters');
}

export function getCharacter(id: string): Promise<CharacterDto> {
	return request(`/api/characters/${id}`);
}

export function createCharacter(name: string, description: string): Promise<CharacterDto> {
	return request('/api/characters', {
		method: 'POST',
		body: JSON.stringify({ name, description })
	});
}

export function updateCharacter(id: string, name: string, description: string): Promise<CharacterDto> {
	return request(`/api/characters/${id}`, {
		method: 'PUT',
		body: JSON.stringify({ name, description })
	});
}

export function createSession(characterId: string): Promise<SessionDto> {
	return request('/api/sessions', {
		method: 'POST',
		body: JSON.stringify({ characterId })
	});
}

export function getActiveSessions(): Promise<ActiveSessionDto[]> {
	return request('/api/sessions');
}

export function getSessionMessages(sessionId: string): Promise<SessionMessageDto[]> {
	return request(`/api/sessions/${sessionId}/messages`);
}

export function addSessionMessage(sessionId: string, mode: string, text: string): Promise<SessionMessageDto> {
	return request(`/api/sessions/${sessionId}/messages`, {
		method: 'POST',
		body: JSON.stringify({ mode, text })
	});
}
