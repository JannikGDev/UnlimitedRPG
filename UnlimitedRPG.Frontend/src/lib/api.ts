export interface CharacterDto {
	id: string;
	name: string;
	description: string;
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

export function createCharacter(name: string, description: string): Promise<CharacterDto> {
	return request('/api/characters', {
		method: 'POST',
		body: JSON.stringify({ name, description })
	});
}
