<script lang="ts">
	import { getCharacters, createCharacter, getActiveSessions } from '$lib/api';
	import type { CharacterDto, ActiveSessionDto } from '$lib/api';

	let characters = $state<CharacterDto[]>([]);
	let sessions = $state<ActiveSessionDto[]>([]);
	let name = $state('');
	let description = $state('');
	let error = $state('');
	let submitting = $state(false);

	$effect(() => {
		getCharacters()
			.then((c) => (characters = c))
			.catch(() => (error = 'Could not reach the server. Is the API running?'));
		getActiveSessions()
			.then((s) => (sessions = s))
			.catch(() => {});
	});

	async function submit() {
		if (!name.trim() || !description.trim()) return;
		submitting = true;
		error = '';
		try {
			const created = await createCharacter(name.trim(), description.trim());
			characters = [...characters, created];
			name = '';
			description = '';
		} catch {
			error = 'Failed to create character.';
		} finally {
			submitting = false;
		}
	}
</script>

<main class="mx-auto max-w-2xl px-4 py-12 space-y-10">
	<h1 class="text-3xl font-bold tracking-wide">UnlimitedRPG</h1>

	<!-- Active sessions -->
	{#if sessions.length > 0}
		<section>
			<h2 class="mb-4 text-lg font-semibold">Active Sessions</h2>
			<ul class="space-y-2">
				{#each sessions as session}
					<li class="hover:bg-gray-600">
						<a href="/session/{session.id}" class="block rounded border p-3">
							<p class="font-medium">{session.characterName}</p>
							<p class="text-sm text-gray-400">Started {new Date(session.startedAt).toLocaleString()}</p>
						</a>
					</li>
				{/each}
			</ul>
		</section>
	{/if}

	<!-- Character list -->
	<section>
		<h2 class="mb-4 text-lg font-semibold">Characters</h2>
		{#if characters.length === 0}
			<p class="text-sm text-gray-500">No characters yet.</p>
		{:else}
			<ul class="space-y-2">
				{#each characters as character}
					<li class="hover:bg-gray-600">
						<a href="/characters/{character.id}" class="block rounded border p-3">
							<p class="font-medium">{character.name}</p>
							<p class="text-sm text-gray-200">{character.description}</p>
						</a>
					</li>
				{/each}
			</ul>
		{/if}
	</section>

	<!-- Creation form -->
	<section>
		<h2 class="mb-4 text-lg font-semibold">Create character</h2>
		<form onsubmit={(e) => { e.preventDefault(); submit(); }} class="space-y-3">
			<input
				type="text"
				placeholder="Name"
				bind:value={name}
				class="w-full rounded border px-3 py-2 text-sm"
			/>
			<textarea
				placeholder="Description"
				bind:value={description}
				rows="3"
				class="w-full rounded border px-3 py-2 text-sm"
			></textarea>
			{#if error}
				<p class="text-sm text-red-500">{error}</p>
			{/if}
			<button
				type="submit"
				disabled={!name.trim() || !description.trim() || submitting}
				class="rounded border px-4 py-2 text-sm font-medium disabled:opacity-40"
			>
				{submitting ? 'Saving…' : 'Save character'}
			</button>
		</form>
	</section>
</main>
