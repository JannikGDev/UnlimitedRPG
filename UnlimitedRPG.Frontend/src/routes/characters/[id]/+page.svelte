<script lang="ts">
	import { goto } from '$app/navigation';
	import { getCharacter, updateCharacter, createSession } from '$lib/api';
	import type { CharacterDto } from '$lib/api';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	let character = $state<CharacterDto | null>(null);
	let error = $state('');
	let editing = $state(false);
	let editName = $state('');
	let editDescription = $state('');
	let saving = $state(false);
	let starting = $state(false);

	$effect(() => {
		getCharacter(data.id)
			.then((c) => (character = c))
			.catch(() => (error = 'Character not found.'));
	});

	function startEdit() {
		editName = character!.name;
		editDescription = character!.description;
		editing = true;
	}

	async function save() {
		if (!editName.trim() || !editDescription.trim()) return;
		saving = true;
		error = '';
		try {
			character = await updateCharacter(data.id, editName.trim(), editDescription.trim());
			editing = false;
		} catch {
			error = 'Failed to save changes.';
		} finally {
			saving = false;
		}
	}

	async function startSession() {
		starting = true;
		error = '';
		try {
			const session = await createSession(data.id);
			goto(`/session/${session.id}`);
		} catch {
			error = 'Failed to start session.';
			starting = false;
		}
	}
</script>

<main class="mx-auto max-w-2xl px-4 py-12 space-y-6">
	<a href="/" class="text-sm hover:underline">← Back</a>

	{#if error && !character}
		<p class="text-sm text-red-500">{error}</p>
	{:else if !character}
		<p class="text-sm text-gray-500">Loading…</p>
	{:else if editing}
		<form onsubmit={(e) => { e.preventDefault(); save(); }} class="space-y-3">
			<input
				type="text"
				bind:value={editName}
				class="w-full rounded border px-3 py-2 text-sm"
			/>
			<textarea
				bind:value={editDescription}
				rows="4"
				class="w-full rounded border px-3 py-2 text-sm"
			></textarea>
			{#if error}
				<p class="text-sm text-red-500">{error}</p>
			{/if}
			<div class="flex gap-2">
				<button
					type="submit"
					disabled={!editName.trim() || !editDescription.trim() || saving}
					class="rounded border px-4 py-2 text-sm font-medium disabled:opacity-40"
				>
					{saving ? 'Saving…' : 'Save'}
				</button>
				<button
					type="button"
					onclick={() => (editing = false)}
					class="rounded border px-4 py-2 text-sm"
				>
					Cancel
				</button>
			</div>
		</form>
	{:else}
		<div class="space-y-4">
			<h1 class="text-3xl font-bold">{character.name}</h1>
			<p class="text-gray-700">{character.description}</p>
			{#if error}
				<p class="text-sm text-red-500">{error}</p>
			{/if}
			<div class="flex gap-2">
				<button
					onclick={startEdit}
					class="rounded border px-4 py-2 text-sm font-medium"
				>
					Edit
				</button>
				<button
					onclick={startSession}
					disabled={starting}
					class="rounded border px-4 py-2 text-sm font-medium disabled:opacity-40"
				>
					{starting ? 'Starting…' : 'Start Session'}
				</button>
			</div>
		</div>
	{/if}
</main>
